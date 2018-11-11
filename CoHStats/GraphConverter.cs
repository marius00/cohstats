using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CoHStats.Aggregator;
using CoHStats.Integration;
using log4net;
using Newtonsoft.Json;

namespace CoHStats {
    public class GraphConverter {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(GraphConverter));

        private readonly Dictionary<Player, List<PlayerStats>> _playerStats = new Dictionary<Player, List<PlayerStats>>();
        // Make a Player aggregate object?
        private readonly Dictionary<Player, bool> _isInvalidPlayer = new Dictionary<Player, bool>();
        private readonly Dictionary<Player, string> _playerNames = new Dictionary<Player, string>();
        private readonly GameReader _gameReader;
        private readonly KillCountAggregator _killCountAggregator = new KillCountAggregator();

        private readonly JsonSerializerSettings _settings;

        public GraphConverter(GameReader gameReader) {
            _settings = new JsonSerializerSettings {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Culture = System.Globalization.CultureInfo.InvariantCulture,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            _gameReader = gameReader;
        }


        private void Add(PlayerStats stats, Player player) {
            if (stats.InfantryKilled < 100000) {
                if (_playerNames.ContainsKey(player)) {
                    _killCountAggregator.Add(stats.TotalKilled, player, _playerNames[player]);
                }

                if (_playerStats.ContainsKey(player)) {
                    _playerStats[player].Add(stats);
                }
            }
            else {
                Logger.Warn($"Invalidating player {player}, detected {stats.InfantryKilled} infantry kills.");
                _isInvalidPlayer[player] = true;
            }
        }

        public void Tick() {
            foreach (var player in new[] {Player.One, Player.Two, Player.Three, Player.Four, Player.Five, Player.Six, Player.Seven, Player.Eight }) {
                // Skip invalids
                if (_isInvalidPlayer.ContainsKey(player)) {
                    continue;
                }

                if (!_playerNames.ContainsKey(player)) {
                    _playerNames[player] = _gameReader.GetPlayerName(player);
                    if (string.IsNullOrEmpty(_playerNames[player])) {
                        Logger.Info($"Player {player} is not valid, not being added to dataset.");
                        _isInvalidPlayer[player] = true;
                    }
                    else {
                        _playerStats[player] = new List<PlayerStats>();
                        Logger.Info($"Player {player} is added as \"{_playerNames[player]}\"");
                    }
                }

                if (!string.IsNullOrEmpty(_playerNames[player])) {

                    // Add new data
                    var stats = _gameReader.FetchStats(player);
                    if (stats != null) {
                        Add(stats, player);
                    }
                }
            }

            _killCountAggregator.Tick();
        }

        private class GraphAggregate {
            public List<GraphMapper> PerPlayerGraph { get; set; }
            public KillCountAggregator.HumanAiKillCountAggregate HumanAiGraph { get; set; }
            public bool IsGameRunning { get; set; }
        }

        public string ToJson() {
            List<GraphMapper> result = new List<GraphMapper>();
            var zeroNode = new GraphNodeDto { x = 0, y = 0 };

            foreach (var player in _playerStats.Keys) {
                var numKills = new List<GraphNodeDto> {zeroNode};

                numKills.AddRange(_playerStats[player].Select((e, idx) => new GraphNodeDto {
                    x = 1 + idx,
                    y = e.InfantryKilled + e.VehiclesDestroyed + e.BuildingsDestroyed
                }));

                GraphMapper entry = new GraphMapper {
                    Graph = new List<List<GraphNodeDto>> { numKills, },
                    Name = _playerNames.ContainsKey(player) ? _playerNames[player] : string.Empty
                };

                result.Add(entry);
            }

            
            return JsonConvert.SerializeObject(
                new GraphAggregate {
                    HumanAiGraph = _killCountAggregator.ToAggregate(),
                    PerPlayerGraph = result,
                    IsGameRunning = _gameReader.IsActive
                }, _settings);
        }
    }
}