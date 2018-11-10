using System.Collections.Generic;
using System.Linq;
using CoHStats.Integration;

namespace CoHStats.Aggregator {
    /**
     * Aggregates kills into "Human VS AI" datasets.
     * Useful for players who only play against the AI.
     */
    public class KillCountAggregator {
        private int _tick;
        private readonly Dictionary<string, List<GraphNodeDto>> _playerKills = new Dictionary<string, List<GraphNodeDto>>();
        private readonly List<GraphNodeDto> _cpuKills = new List<GraphNodeDto>();
        private readonly Dictionary<string, int> _killsPerDistinctPlayer = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _killsPerDistinctCpu = new Dictionary<string, int>();

        public KillCountAggregator() {
            Tick();
        }

        private bool IsCpu(string name) => name.Contains("CPU") && name.Contains("-");

        public void Add(int totalKills, string name) {
            if (!string.IsNullOrEmpty(name)) {
                if (!IsCpu(name)) {
                    _killsPerDistinctPlayer[name] = totalKills;
                }
                else {
                    _killsPerDistinctCpu[name] = totalKills;
                }
            }

            
        }

        public void Tick() {
            _cpuKills.Add(new GraphNodeDto {
                x = _tick,
                y = _killsPerDistinctCpu.Sum(e => e.Value)
            });

            foreach (var player in _killsPerDistinctPlayer.Keys) {
                if (!_playerKills.ContainsKey(player)) {
                    _playerKills[player] = new List<GraphNodeDto> {
                        new GraphNodeDto {
                            x = _tick,
                            y = 0
                        }
                    };
                }

                _playerKills[player].Add(new GraphNodeDto {
                    x = _tick,
                    y = _killsPerDistinctPlayer[player]
                });
            }

            _tick++;
        }

        public class HumanAiKillCountAggregate {
            public List<List<GraphNodeDto>> PlayerKills { get; set; }
            public List<List<GraphNodeDto>> CpuKills { get; set; }
            public string PlayerLabel { get; set; }
        }

        public HumanAiKillCountAggregate ToAggregate() {
            
            return new HumanAiKillCountAggregate {
                CpuKills = new List<List<GraphNodeDto>> { _cpuKills },
                PlayerKills = _playerKills.Select(p => p.Value).ToList(),
                PlayerLabel = string.Join(" - ", _killsPerDistinctPlayer.Select(p => $"{p.Key}: {p.Value}"))
            };
        }
    }
}
