using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoHStats {
    class GameReader {
        private const int ProcessWmRead = 0x0010;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize,
            ref int lpNumberOfBytesRead);

        private readonly List<int> _offsets = new List<int> {
            0x901EB8,
            0x748,
            0x70,
            0x4,
            0x9C //, 0x194
        };


        private IntPtr _processHandle;
        private int _ptr = 0;

        private void Initialize() {
            var candidates = Process.GetProcessesByName("RelicCOH");
            Process process = candidates[0];
            _processHandle = OpenProcess(ProcessWmRead, false, process.Id);

            int bytesRead = 0;
            byte[] buffer = new byte[4];

            _ptr = process.MainModule.BaseAddress.ToInt32();
            foreach (var offset in _offsets) {
                if (!ReadProcessMemory((int) _processHandle, _ptr + offset, buffer, buffer.Length, ref bytesRead)) {
                    Console.WriteLine($"Error reading from address {_ptr + offset}");
                }
                else {
                    _ptr = BitConverter.ToInt32(buffer, 0);
                }
            }
        }

        public PlayerStats FetchStats(Player player) {
            if (_ptr == 0) {
                Initialize();
            }
            
            int bytesRead = 0;
            byte[] buffer = new byte[4];

            ReadProcessMemory((int) _processHandle, _ptr + (int)player, buffer, buffer.Length, ref bytesRead);
            int numKills = BitConverter.ToInt32(buffer, 0);
            return new PlayerStats {
                InfantryKilled = numKills
            };
        }
    }
}