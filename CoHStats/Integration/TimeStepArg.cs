using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoHStats.Integration {
    public class TimeStepArg : EventArgs {
        public int StepSize { get; set; }
    }
}
