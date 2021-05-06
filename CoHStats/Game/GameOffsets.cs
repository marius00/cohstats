using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoHStats.Game {
    /// <summary>
    /// Offsets for reading player specific statistics
    /// Found using Cheat Engine and pointer maps
    /// </summary>
    public static class GameOffsets {
        public const int Name = -0x184;
        public const int BuildingsLost = -0x7C;
        public const int VehiclesLost = -0x78;
        public const int InfantryLost = -0x74;
        public const int InfantryKills = 0; // Our base pointer starts at inf kills, since its what was used as a baseline in CheatEngine
        public const int VehicleKills = 8;
        public const int BuildingKills = 16;

        /// <summary>
        /// Offsets required to follow the pointers to the player data.
        /// Typically starting at the main module base address, following pointers are specific locations.
        /// 
        /// </summary>
        public static readonly List<int> PlayerPointerChain = new List<int> {
            0x901EB8,
            0x748,
            0x70,
            0x4,
            0x9C //, 0x194
        };

        
    }
}
