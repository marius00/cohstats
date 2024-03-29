﻿using System;
using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<Player, List<PlayerStats>> _playerStats = new ConcurrentDictionary<Player, List<PlayerStats>>();
        private readonly ConcurrentDictionary<Player, List<PlayerStats>> _cpuStats = new ConcurrentDictionary<Player, List<PlayerStats>>();
        private readonly ConcurrentDictionary<Player, List<PlayerStats>> _playerDeltaStats = new ConcurrentDictionary<Player, List<PlayerStats>>();
        private readonly ConcurrentDictionary<Player, List<PlayerStats>> _cpuDeltaStats = new ConcurrentDictionary<Player, List<PlayerStats>>();
        private readonly GameReader _gameReader;
        private readonly PlayerService _playerService;
        private const int DeltaOffset = 15; // How many seconds back should the delta count

        public DataAggregator(GameReader gameReader, PlayerService playerService) {
            _gameReader = gameReader;
            _playerService = playerService;
        }


        /// <summary>
        /// Gets the "delta" of the stats.
        /// Eg if last tick had 100 kills and this tick has 105, then it will return it as 5 kills the past tick.
        ///
        /// If this is the first tick, the supplied entry will be returned
        /// </summary>
        /// <param name="stats">The stats from the current tick</param>
        /// <param name="recentStats">List of the recent stats (totals)</param>
        /// <returns></returns>
        private PlayerStats GetDelta(PlayerStats stats, List<PlayerStats> recentStats) {
            if (recentStats.Count == 0) {
                return stats;
            }
            else {
                // Minus the offset to get "recent kills over X seconds"
                var offset = Math.Max(0, recentStats.Count - DeltaOffset);

                var prior = recentStats[offset];
                return new PlayerStats {
                    BuildingsLost = stats.BuildingsLost - prior.BuildingsLost,
                    InfantryLost = stats.InfantryLost - prior.InfantryLost,
                    VehiclesLost = stats.VehiclesLost - prior.VehiclesLost,
                    BuildingsDestroyed = stats.BuildingsDestroyed - prior.BuildingsDestroyed,
                    InfantryKilled = stats.InfantryKilled - prior.InfantryKilled,
                    VehiclesDestroyed = stats.VehiclesDestroyed - prior.VehiclesDestroyed
                };
            }
        }

#if DEBUG
// TODO: Make a mock GameHandler or something instead of polluting real code
        public void AddDebugData() {
            /*EnsureExists(Player.One);
            int[] numKills = new[] { 0, 0, 0, 0, 5, 15, 18, 19, 20, 21, 25, 30, 31, 32, 33, 34, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35 };
            int[] numLosses= new[] { 0, 0, 0, 2, 4, 4, 4, 4, 13, 13, 14, 14, 15, 15, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22 };
            for (int i = 0; i < numLosses.Length; i++) {
                var s = new PlayerStats {
                    InfantryLost = numLosses[i],
                    InfantryKilled = numKills[i],
                    BuildingsLost = i
                };

                _playerDeltaStats[Player.One].Add(GetDelta(s, _playerStats[Player.One]));
                _playerStats[Player.One].Add(s);
            }*/
        }

        private void AddDebugData(List<JsonExportFormat> exports) {
            /*if (_playerService.GetPlayers().Count == 0 && _playerStats.Count > 0 && _playerStats[Player.One].Count > 0) {
                EnsureExists(Player.One);

                exports.Add(new JsonExportFormat {
                    Name = _playerService.GetName(Player.One),
                    Stats = _playerStats[Player.One].ToList(),
                    Deltas = _playerDeltaStats[Player.One].ToList() // Copy to prevent mutation during serialization
                });
            }*/
        }
#else
        private void AddDebugData(List<JsonExportFormat> exports) {}
#endif

        public void Tick() {
            foreach (var player in _playerService.GetPlayers()) {
                var tickStats = _gameReader.FetchStats(player);
                if (tickStats == null) {
                    _playerService.Invalidate(player);
                    continue;
                }

                

                EnsureExists(player);
                var totals = _playerService.IsCpu(player) ? _cpuStats : _playerStats;
                var deltas = _playerService.IsCpu(player) ? _cpuDeltaStats : _playerDeltaStats;
                var delta = GetDelta(tickStats, totals[player]);

                totals[player].Add(tickStats);
                deltas[player].Add(delta);
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

            AddDebugData(exports);
            return exports;
        }

        /// <summary>
        /// Ensures that we don't run into any nullpointers on dictionary lookups, creates lists as needed.
        /// </summary>
        /// <param name="player"></param>
        private void EnsureExists(Player player) {
            if (_playerService.IsCpu(player)) {
                if (!_cpuStats.ContainsKey(player)) {
                    _cpuStats.TryAdd(player, new List<PlayerStats>());
                }
                if (!_cpuDeltaStats.ContainsKey(player)) {
                    _cpuDeltaStats.TryAdd(player, new List<PlayerStats>());
                }
            }
            else {
                if (!_playerStats.ContainsKey(player)) {
                    _playerStats.TryAdd(player, new List<PlayerStats>());
                }
                if (!_playerDeltaStats.ContainsKey(player)) {
                    _playerDeltaStats.TryAdd(player, new List<PlayerStats>());
                }
            }
        }
    }
}
