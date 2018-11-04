using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoHStats.Integration {
    public class UpdatePlayerArg : EventArgs {
        public Player Player { get; set; }
    }
}
