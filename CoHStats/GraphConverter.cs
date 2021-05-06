using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CoHStats.Aggregator;
using CoHStats.Game;
using CoHStats.Integration;
using log4net;
using Newtonsoft.Json;

namespace CoHStats {
    public class GraphConverter {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(GraphConverter));

        private readonly Dictionary<Player, List<PlayerStats>> _playerStats = new Dictionary<Player, List<PlayerStats>>();
        private readonly Dictionary<Player, int> _prevTotalPlayerKills = new Dictionary<Player, int>();
        // Make a Player aggregate object?
        private readonly GameReader _gameReader;
        private readonly KillCountAggregator _killCountAggregator = new KillCountAggregator();

        private readonly JsonSerializerSettings _settings;
        private readonly int _resolution;
        private readonly PlayerService _playerService;

        public GraphConverter(GameReader gameReader, int resolution, PlayerService playerService) {
            _settings = new JsonSerializerSettings {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Culture = System.Globalization.CultureInfo.InvariantCulture,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            _gameReader = gameReader;
            _resolution = resolution;
            _playerService = playerService;
        }


        private void Add(PlayerStats stats, Player player) {

            if (stats.InfantryKilled < 100000) {
                
                if (_playerService.IsValid(player)) {
                    _killCountAggregator.Add(stats.TotalKilled, player, _playerService.GetName(player));
                }


                if (_playerStats.ContainsKey(player)) {
                    if (_prevTotalPlayerKills.ContainsKey(player) && _prevTotalPlayerKills[player] != stats.TotalKilled) {
                        Logger.Debug($"Player {player} has {stats.InfantryKilled} inf, {stats.VehiclesDestroyed} vehicle, {stats.BuildingsDestroyed} building kills");
                    }

                    _playerStats[player].Add(stats);
                    _prevTotalPlayerKills[player] = stats.TotalKilled;
                }
            }
            else {
                Logger.Warn($"Invalidating player {player}, detected {stats.InfantryKilled} infantry kills.");
                _playerService.Invalidate(player);
            }
        }

        public void Tick() {
            foreach (var player in _playerService.GetPlayers()) {
                if (!_playerStats.ContainsKey(player)) {
                    _playerStats[player] = new List<PlayerStats>();
                    Logger.Info($"Player {player} is added as \"{_playerService.GetName(player)}\"");
                }

                // Add new data
                var stats = _gameReader.FetchStats(player);
                if (stats != null) {
                    Add(stats, player);
                }
            }

            _killCountAggregator.Tick();
        }

        private class GraphAggregate {
            public List<GraphMapper> PerPlayerGraph { get; set; }
            public KillCountAggregator.HumanAiKillCountAggregate HumanAiGraph { get; set; }
            public bool IsGameRunning { get; set; }
        }

        private int FormatKills(int numKills) {
            if (_resolution > 1)
                return numKills - (numKills % _resolution);
            else
                return numKills;
        }

        public string ToJson() {

            // Calc per-player graph, useless?
            List<GraphMapper> result = new List<GraphMapper>();
            var zeroNode = new GraphNodeDto { x = 0, y = 0 };
            foreach (var player in _playerStats.Keys) {
                var numKills = new List<GraphNodeDto> {zeroNode};

                numKills.AddRange(_playerStats[player].Select((e, idx) => new GraphNodeDto {
                    x = 1 + idx,
                    y = FormatKills(e.TotalKilled)
                }));

                GraphMapper entry = new GraphMapper {
                    Graph = new List<List<GraphNodeDto>> { numKills, },
                    Name = _playerService.GetName(player)
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