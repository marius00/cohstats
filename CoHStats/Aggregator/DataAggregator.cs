using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CoHStats.Game;
using CoHStats.Integration;
using log4net;

namespace CoHStats.Aggregator {
    /**
     * Aggregates kills into "Human VS AI" datasets.
     * Useful for players who only play against the AI.
     */
    public class DataAggregator {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DataAggregator));
        private readonly Dictionary<Player, List<PlayerStats>> _playerStats = new Dictionary<Player, List<PlayerStats>>();
        private readonly Dictionary<Player, List<PlayerStats>> _cpuStats = new Dictionary<Player, List<PlayerStats>>();
        private readonly Dictionary<Player, List<PlayerStats>> _playerDeltaStats = new Dictionary<Player, List<PlayerStats>>();
        private readonly Dictionary<Player, List<PlayerStats>> _cpuDeltaStats = new Dictionary<Player, List<PlayerStats>>();
        private readonly GameReader _gameReader;
        private readonly PlayerService _playerService;

        public DataAggregator(GameReader gameReader, PlayerService playerService) {
            _gameReader = gameReader;
            _playerService = playerService;
            Tick();
        }

        /// <summary>
        /// Gets the "delta" of the stats.
        /// Eg if last tick had 100 kills and this tick has 105, then it will return it as 5 kills the past tick.
        ///
        /// If this is the first tick, the supplied entry will be returned
        /// </summary>
        /// <param name="stats">The stats from the current tick</param>
        /// <param name="recentStats">List of the recent stats</param>
        /// <returns></returns>
        private PlayerStats GetDelta(PlayerStats stats, List<PlayerStats> recentStats) {
            if (recentStats.Count == 0) {
                return stats;
            }
            else {
                var recent = recentStats[recentStats.Count - 1];
                return new PlayerStats {
                    BuildingsLost = stats.BuildingsLost - recent.BuildingsLost,
                    InfantryLost = stats.InfantryLost - recent.InfantryLost,
                    VehiclesLost = stats.VehiclesLost - recent.VehiclesLost,
                    BuildingsDestroyed = stats.BuildingsDestroyed - recent.BuildingsDestroyed,
                    InfantryKilled = stats.InfantryKilled - recent.InfantryKilled,
                    VehiclesDestroyed = stats.VehiclesDestroyed - recent.VehiclesDestroyed
                };
            }
        }
        
        public void Tick() {
            var playerCollections = new[] { _playerStats, _playerDeltaStats };
            var cpuCollections = new[] { _cpuStats, _cpuDeltaStats };

            foreach (var player in _playerService.GetPlayers()) {
                var stats = _gameReader.FetchStats(player);
                if (stats == null) {
                    continue;
                }

                EnsureExists(player);
                var active = _playerService.IsCpu(player) ? cpuCollections : playerCollections;

                active[0][player].Add(stats);

                var delta = GetDelta(stats, active[0][player]);
                active[1][player].Add(delta);
            }
        }

        /// <summary>
        /// Ensures that we don't run into any nullpointers on dictionary lookups, creates lists as needed.
        /// </summary>
        /// <param name="player"></param>
        private void EnsureExists(Player player) {
            if (_playerService.IsCpu(player)) {
                if (!_cpuStats.ContainsKey(player)) {
                    _cpuStats.Add(player, new List<PlayerStats>());
                }
                if (!_cpuDeltaStats.ContainsKey(player)) {
                    _cpuDeltaStats.Add(player, new List<PlayerStats>());
                }
            }
            else {
                if (!_playerStats.ContainsKey(player)) {
                    _playerStats.Add(player, new List<PlayerStats>());
                }
                if (!_playerDeltaStats.ContainsKey(player)) {
                    _playerDeltaStats.Add(player, new List<PlayerStats>());
                }
            }
        }
    }
}
