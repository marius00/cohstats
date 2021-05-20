using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using log4net;

namespace CoHStats.Game {
    /// <summary>
    /// Responsible for reading information for the game.
    /// Uses direct memory lookup (not functions hooks)
    ///
    /// Will attempt to locate players based on known pointer chains, and read information about the players from specific offsets.
    ///
    /// Once in a while (rarely) it'll fail. No idea why.
    /// </summary>
    public class GameReader {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(GameReader));
        private const int ProcessWmRead = 0x0010;


        private IntPtr _processHandle;
        private int _ptr = 0;

        private bool Initialize() {
            var candidates = Process.GetProcessesByName("RelicCOH");
            if (candidates.Length == 0) {
                return false;
            }

            try {
                Logger.Info("Connecting to CoH and attempting to discover players..");

                Process process = candidates[0];
                _processHandle = Util.OpenProcess(ProcessWmRead, false, process.Id);


                int bytesRead = 0;
                byte[] buffer = new byte[4];

                _ptr = process.MainModule.BaseAddress.ToInt32();
                foreach (var offset in GameOffsets.PlayerPointerChain) {
                    if (!Util.ReadProcessMemory((int) _processHandle, _ptr + offset, buffer, buffer.Length, ref bytesRead)) {
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
                    GetPlayerName((Player)player);
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
            var pointerToName = _ptr + (int)player + GameOffsets.Name;
            byte[] nameBuffer = new byte[64];
            if (!Util.ReadProcessMemory((int)_processHandle, pointerToName, nameBuffer, nameBuffer.Length, ref bytesRead)) {
                Logger.Warn($"Error reading name from address {pointerToName}");
            }
            else {
                string result = System.Text.Encoding.Unicode.GetString(nameBuffer, 0, Util.GetUnicodeStringLength(nameBuffer)).Replace("\0", "");
                var isValidName = Util.IsValidString(result);
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

        public void Invalidate() {
            _ptr = 0;
        }

        /// <summary>
        /// Read a specific memory offset
        /// Assumes the target offset is a 4 bit integer.
        /// </summary>
        /// <param name="player">Player</param>
        /// <param name="offset">Offset from GameOffsets</param>
        /// <returns></returns>
        private int ReadOffset(Player player, int offset) {
            int bytesRead = 0;
            byte[] buffer = new byte[4];
            if (!Util.ReadProcessMemory((int)_processHandle, _ptr + (int)player + offset, buffer, buffer.Length, ref bytesRead)) {
                Logger.Debug($"Failed to read number of buildings destroyed for player {player}");
                throw new ArgumentException($"Failed reading pointer for player {player}, offset {offset}");
            }

            return BitConverter.ToInt32(buffer, 0);
        }

        public PlayerStats FetchStats(Player player) {
            if (_ptr == 0) {
                if (!Initialize()) {
                    return null;
                }
            }

            try {
                int numInfantryKills = ReadOffset(player, GameOffsets.InfantryKills);
                int numVehicleKills = ReadOffset(player, GameOffsets.VehicleKills);
                int numBuildingsDestroyed = ReadOffset(player, GameOffsets.BuildingKills);

                int numInfantryLost = ReadOffset(player, GameOffsets.InfantryLost);
                int numVehiclesLost = ReadOffset(player, GameOffsets.VehiclesLost);
                int numBuildingsLost = ReadOffset(player, GameOffsets.BuildingsLost);

                // Game probably closed, reading garbage data.
                if (numInfantryKills > 10000 || numVehicleKills > 10000 || numBuildingsDestroyed > 10000)
                    return null;

                return new PlayerStats {
                    InfantryKilled = numInfantryKills,
                    VehiclesDestroyed = numVehicleKills,
                    BuildingsDestroyed = numBuildingsDestroyed,

                    InfantryLost = numInfantryLost,
                    VehiclesLost = numVehiclesLost,
                    BuildingsLost = numBuildingsLost,
                };
            }
            catch (ArgumentException ex) {
                Logger.Warn(ex.Message, ex);
                if (player == Player.One) {
                    Logger.Warn("Error reading player one, invalidating instance");
                    _ptr = 0;
                }
                return null;
            }
        }
    }
}