﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using log4net;

namespace CoHStats {
    public class GameReader {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(GameReader));
        private const int ProcessWmRead = 0x0010;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        private readonly List<int> _offsets = new List<int> {
            0x901EB8,
            0x748,
            0x70,
            0x4,
            0x9C //, 0x194
        };


        private IntPtr _processHandle;
        private int _ptr = 0;


        private static int GetUnicodeStringLength(byte[] data) {
            byte prevByte = 0x0;
            for (int i = 0; i < data.Length; i++) {
                if (data[i] == 0x0 && prevByte == 0x0) {
                    return i;
                }
                prevByte = data[i];
            }

            return 0;
        }

        private static bool IsValidString(string s) {
            Regex r = new Regex("[^A-Za-z0-9$ ]$");
            return s.Length > 1 && !r.IsMatch(s.Substring(0, 1));
        }

        private bool Initialize() {
            var candidates = Process.GetProcessesByName("RelicCOH");
            if (candidates.Length == 0) {
                return false;
            }

            try {
                Logger.Info("Connecting to CoH and attempting to discover players..");

                Process process = candidates[0];
                _processHandle = OpenProcess(ProcessWmRead, false, process.Id);


                int bytesRead = 0;
                byte[] buffer = new byte[4];

                _ptr = process.MainModule.BaseAddress.ToInt32();
                foreach (var offset in _offsets) {
                    if (!ReadProcessMemory((int) _processHandle, _ptr + offset, buffer, buffer.Length, ref bytesRead)) {
                        Logger.Warn($"Error reading from address 0x{_ptr + offset}");
                        _ptr = 0;
                        return false;
                    }
                    else {
                        _ptr = BitConverter.ToInt32(buffer, 0);
                        Logger.Debug($"Reading from address 0x{_ptr + offset:X8}");
                    }
                }
                
                foreach (var player in Enum.GetValues(typeof(Player))) {
                    Logger.Info($"Detected player {player} at memory offset 0x{(_ptr + (int) player):X8}");

                    var pointerToName = _ptr + (int) player - 0x184;
                    byte[] nameBuffer = new byte[64];
                    if (!ReadProcessMemory((int) _processHandle, pointerToName, nameBuffer, nameBuffer.Length, ref bytesRead)) {
                        Logger.Warn($"Error reading name from address 0x{pointerToName}");
                    }
                    else {
                        string result = System.Text.Encoding.Unicode.GetString(nameBuffer, 0, GetUnicodeStringLength(nameBuffer)).Replace("\0", "");
                        var isValidName = IsValidString(result);
                        Logger.Debug($"Player {player} is named \"{result}\", IsValid: {isValidName}");
                    }
                }

                Logger.Info("Initialization complete.");
            }
            catch (Win32Exception ex) {
                _processHandle = IntPtr.Zero;
                _ptr = 0;
                Logger.Warn(ex.Message, ex);
                return false;
            }

            return true;
        }

        public string GetPlayerName(Player player) {
            int bytesRead = 0;
            var pointerToName = _ptr + (int)player - 0x184;
            byte[] nameBuffer = new byte[64];
            if (!ReadProcessMemory((int)_processHandle, pointerToName, nameBuffer, nameBuffer.Length, ref bytesRead)) {
                Logger.Warn($"Error reading name from address {pointerToName}");
            }
            else {
                string result = System.Text.Encoding.Unicode.GetString(nameBuffer, 0, GetUnicodeStringLength(nameBuffer)).Replace("\0", "");
                var isValidName = IsValidString(result);
                Logger.Debug($"Player {player} is named \"{result}\", IsValid: {isValidName}");

                if (isValidName) {
                    return result;
                }
            }

            return string.Empty;
        }

        public bool IsActive {
            get {
                if (_ptr == 0) {
                    Initialize();
                }

                return _ptr != 0;
            }
        }

        public PlayerStats FetchStats(Player player) {
            if (_ptr == 0) {
                if (!Initialize()) {
                    return null;
                }
            }
            
            int bytesRead = 0;
            byte[] buffer = new byte[4];

            if (!ReadProcessMemory((int) _processHandle, _ptr + (int)player, buffer, buffer.Length, ref bytesRead)) {
                Logger.Debug($"Failed to read number of infantry kills for player {player}");
                if (player == Player.One) {
                    Logger.Warn($"Failed to read infantry kills for player One, resetting state to uninitialized.");
                    _ptr = 0;
                }
                return null;
            }
            int numKills = BitConverter.ToInt32(buffer, 0);

            if (!ReadProcessMemory((int)_processHandle, _ptr + (int)player + 8, buffer, buffer.Length, ref bytesRead)) {
                Logger.Debug($"Failed to read number of vehicle kills for player {player}");
                return null;
            }
            int numVehicleKills = BitConverter.ToInt32(buffer, 0);

            if (!ReadProcessMemory((int)_processHandle, _ptr + (int)player + 16, buffer, buffer.Length, ref bytesRead)) {
                Logger.Debug($"Failed to read number of buildings destroyed for player {player}");
                return null;
            }
            int numBuildingsDestroyed = BitConverter.ToInt32(buffer, 0);

            /*if (!ReadProcessMemory((int)_processHandle, _ptr + (int)player + 20, buffer, buffer.Length, ref bytesRead)) {
                return null;
            }*/

            return new PlayerStats {
                InfantryKilled = numKills,
                VehiclesDestroyed = numVehicleKills,
                BuildingsDestroyed = numBuildingsDestroyed
            };
        }
    }
}