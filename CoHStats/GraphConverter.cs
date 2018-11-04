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

        public string ToJson(Player player, int stepSize) {
            if (!_playerStats.ContainsKey(player)) {
                _playerStats[player] = new List<PlayerStats>();
            }
            var dataset = _playerStats[player];

            var infantryKills = new List<GraphNodeDto>();
            var vehicleKills = new List<GraphNodeDto>();
            var buildingKills = new List<GraphNodeDto>();

            int prev = 0;
            for (int i = 0; i < dataset.Count; i+= stepSize) { // Since we got the 'total', aggregating data into steps is as easy as skippingover.
                // If dataset.length % 3 != 0, then this ensures we don't go over bounds in the array.
                var idx = Math.Min(i, dataset.Count - 1);

                infantryKills.Add(new GraphNodeDto {
                        x = idx,
                        y = dataset[idx].InfantryKilled - prev // Delta
                    }
                );

                vehicleKills.Add(new GraphNodeDto {
                        x = idx,
                        y = 0
                    }
                );

                buildingKills.Add(new GraphNodeDto {
                        x = idx,
                        y = 0
                    }
                );

                prev = dataset[idx].InfantryKilled;
            }

            var result = new List<List<GraphNodeDto>>() {
                infantryKills,
                vehicleKills,
                buildingKills
            };

            return JsonConvert.SerializeObject(result, _settings);
        }
    }
}