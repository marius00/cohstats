using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
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
                _playerStats[player].Add(stats);
            }
            else {
                Logger.Warn($"Invalidating player {player}, detected {stats.InfantryKilled} infantry kills.");
                _isInvalidPlayer[player] = true;
            }
        }

        public void Tick() {
            foreach (var player in new[] {Player.One, Player.Two, Player.Three, Player.Four}) {
                // Skip invalids
                if (_isInvalidPlayer.ContainsKey(player)) {
                    continue;
                }

                if (!_playerStats.ContainsKey(player)) {
                    _playerStats[player] = new List<PlayerStats>();
                }

                if (!_playerNames.ContainsKey(player)) {
                    _playerNames[player] = _gameReader.GetPlayerName(player);
                    if (string.IsNullOrEmpty(_playerNames[player])) {
                        _isInvalidPlayer[player] = true;
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
        }

        public string ToJson() {
            List<GraphMapper> result = new List<GraphMapper>();

            foreach (var player in new[] {Player.One, Player.Two, Player.Three, Player.Four}) {
                if (!_playerStats.ContainsKey(player)) {
                    _playerStats[player] = new List<PlayerStats>();
                }

                var dataset = _playerStats[player];

                var zeroNode = new GraphNodeDto {
                    x = 0,
                    y = 0
                };

                var infantryKills = new List<GraphNodeDto> {zeroNode};
                var vehicleKills = new List<GraphNodeDto> {zeroNode};
                var buildingKills = new List<GraphNodeDto> {zeroNode};

                infantryKills.AddRange(dataset.Select((e, idx) => new GraphNodeDto {
                    x = 1 + idx,
                    y = e.InfantryKilled
                }));

                vehicleKills.AddRange(dataset.Select((e, idx) => new GraphNodeDto {
                    x = 1 + idx,
                    y = e.VehiclesDestroyed
                }));

                buildingKills.AddRange(dataset.Select((e, idx) => new GraphNodeDto {
                    x = 1 + idx,
                    y = e.BuildingsDestroyed
                }));

                GraphMapper entry = new GraphMapper {
                    Graph = new List<List<GraphNodeDto>> {
                        infantryKills,
                        vehicleKills,
                        buildingKills
                    },
                    IsValidPlayer = !_isInvalidPlayer.ContainsKey(player) && _playerNames.ContainsKey(player) && !string.IsNullOrEmpty(_playerNames[player]),
                    Name = _playerNames.ContainsKey(player) ? _playerNames[player] : string.Empty
                };

                result.Add(entry);
            }

            return JsonConvert.SerializeObject(result, _settings);
        }
    }
}