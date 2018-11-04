using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoHStats.Integration;
using Newtonsoft.Json;

namespace CoHStats {
    public class GraphConverter {
        private readonly Dictionary<Player, List<PlayerStats>> _playerStats =
            new Dictionary<Player, List<PlayerStats>>();

        private readonly JsonSerializerSettings _settings;

        public GraphConverter() {
            _settings = new JsonSerializerSettings {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Culture = System.Globalization.CultureInfo.InvariantCulture,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
        }


        public void Add(PlayerStats stats, Player player) {
            if (!_playerStats.ContainsKey(player)) {
                _playerStats[player] = new List<PlayerStats>();
            }

            _playerStats[player].Add(stats);
        }

        public string ToJson(int stepSize) {
            List<List<List<GraphNodeDto>>> result = new List<List<List<GraphNodeDto>>>();
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

                int prevInfantryKilled = 0;
                int prevVehiclesKilled = 0;
                int prevBuildingsDestroyed = 0;
                for (int i = 0; i < dataset.Count; i += stepSize) {
                    // Since we got the 'total', aggregating data into steps is as easy as skippingover.
                    // If dataset.length % 3 != 0, then this ensures we don't go over bounds in the array.
                    var idx = Math.Min(i, dataset.Count - 1);

                    infantryKills.Add(new GraphNodeDto {
                            x = 1 + idx,
                            y = dataset[idx].InfantryKilled - prevInfantryKilled // Delta
                        }
                    );

                    vehicleKills.Add(new GraphNodeDto {
                            x = 1 + idx,
                            y = dataset[idx].VehiclesDestroyed - prevVehiclesKilled // Delta
                        }
                    );

                    buildingKills.Add(new GraphNodeDto {
                            x = 1 + idx,
                            y = dataset[idx].BuildingsDestroyed - prevBuildingsDestroyed // Delta
                        }
                    );

                    prevInfantryKilled = dataset[idx].InfantryKilled;
                    prevVehiclesKilled = dataset[idx].VehiclesDestroyed;
                    prevBuildingsDestroyed = dataset[idx].BuildingsDestroyed;
                }

                result.Add(new List<List<GraphNodeDto>> {
                    infantryKills,
                    vehicleKills,
                    buildingKills
                });
            }

            return JsonConvert.SerializeObject(result, _settings);
        }
    }
}