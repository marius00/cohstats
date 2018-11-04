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
        public event EventHandler OnUpdatePlayer;
        public event EventHandler OnUpdateTimeAggregation;
        public string GraphJson { get; set; }

        public void setTimeAggregation(string arg) {
            int stepSize = int.Parse(arg);
            OnUpdateTimeAggregation?.Invoke(this, new TimeStepArg { StepSize = stepSize});
            Logger.Info($"Aggregation interval now set to {stepSize} seconds");
        }

        public void setPlayer(string arg) {
            int idx = int.Parse(arg);
            switch (idx) {
                case 1:
                    Logger.Info("Updating active player to player One");
                    OnUpdatePlayer?.Invoke(this, new UpdatePlayerArg {Player = Player.One});
                    break;

                case 2:
                    Logger.Info("Updating active player to player Two");
                    OnUpdatePlayer?.Invoke(this, new UpdatePlayerArg {Player = Player.Two});
                    break;

                case 3:
                    Logger.Info("Updating active player to player Three");
                    OnUpdatePlayer?.Invoke(this, new UpdatePlayerArg {Player = Player.Three});
                    break;

                case 4:
                    Logger.Info("Updating active player to player Four");
                    OnUpdatePlayer?.Invoke(this, new UpdatePlayerArg {Player = Player.Four});
                    break;

                default:
                    Logger.Warn($"setPlayer called with arg \"{idx}\", which is not handled");
                    break;
            }
        }
    }
}