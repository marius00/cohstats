using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoHStats {
    public class PlayerStats {
        public int TotalKilled => InfantryKilled + VehiclesDestroyed + BuildingsDestroyed;
        public int InfantryKilled { get; set; }
        public int VehiclesDestroyed { get; set; }
        public int BuildingsDestroyed { get; set; }

        public int InfantryLost { get; set; }
        public int VehiclesLost { get; set; }
        public int BuildingsLost { get; set; }
    }
}
