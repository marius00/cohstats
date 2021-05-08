using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.WebSockets;
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
        private const int DeltaOffset = 6; // How many seconds back should the delta count

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
                // -1 since we just inserted an entry prior to GetDelta being called
                // Minus the offset again to get "recent kills over X seconds"
                var offset = Math.Max(0, recentStats.Count - 1 - DeltaOffset);

                var recent = recentStats[offset];
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

        // TODO: No reason this class should be responsible for export formats.. but its the owner of the data so fits really well.
        public List<JsonExportFormat> Export() {
            List<JsonExportFormat> exports = new List<JsonExportFormat>(2);
            foreach (var player in _playerService.GetPlayers()) {
                if (_playerService.IsCpu(player))
                    continue;

                EnsureExists(player);

                exports.Add(new JsonExportFormat {
                    Name = _playerService.GetName(player),
                    Stats = _playerStats[player].ToList(),
                    Deltas = _playerDeltaStats[player].ToList() // Copy to prevent mutation during serialization
                });
            }

            return exports;
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
