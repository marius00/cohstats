using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoHStats.Game {
    static class Util {

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        public static bool IsValidString(string s) {
            Regex r = new Regex("[^A-Za-z0-9$ ]$");
            return s.Length > 1 && !r.IsMatch(s.Substring(0, 1));
        }

        public static int GetUnicodeStringLength(byte[] data) {
            byte prevByte = 0x0;
            for (int i = 0; i < data.Length; i++) {
                if (data[i] == 0x0 && prevByte == 0x0) {
                    return i;
                }
                prevByte = data[i];
            }

            return 0;
        }

    }
}
