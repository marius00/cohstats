using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoHStats.Integration {
    public class JsonExportFormat {
        public string Name { get; set; }
        public List<PlayerStats> Stats;
        public List<PlayerStats> Deltas;
    }
}
