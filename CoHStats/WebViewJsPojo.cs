using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoHStats.Integration;
using log4net;
using Newtonsoft.Json;

namespace CoHStats {
    public class WebViewJsPojo {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(WebViewJsPojo));
        public event EventHandler OnUpdateTimeAggregation;
        public string GraphJson { get; set; }

        public void setTimeAggregation(string arg) {
            int stepSize = int.Parse(arg);
            OnUpdateTimeAggregation?.Invoke(this, new TimeStepArg { StepSize = stepSize});
            Logger.Info($"Aggregation interval now set to {stepSize} seconds");
        }

    }
}