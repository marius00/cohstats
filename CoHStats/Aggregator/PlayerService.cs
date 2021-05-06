using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoHStats.Game;
using log4net;

namespace CoHStats.Aggregator {
    public class PlayerService {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PlayerService));
        private readonly GameReader _gameReader;
        private readonly Dictionary<Player, bool> _invalidPlayers = new Dictionary<Player, bool>();
        private readonly Dictionary<Player, string> _detectedPlayers = new Dictionary<Player, string>();

        public PlayerService(GameReader gameReader) {
            _gameReader = gameReader;
        }

        /// <summary>
        /// Invalidate a given player, if it's discovered to be invalid.
        /// Example: Player has 2 million infantry kills.
        /// </summary>
        /// <param name="player"></param>
        public void Invalidate(Player player) {
            Logger.Info($"Invalidating player {player}");
            _invalidPlayers[player] = true;

            if (player == Player.One) {
                _gameReader.Invalidate();
                // TODO: Now what, will this class be recreated?
            }
        }

        public List<Player> GetPlayers() {
            foreach (var player in new[] {
                Player.One, Player.Two, Player.Three, Player.Four, Player.Five, Player.Six, Player.Seven, Player.Eight
            }) {
                // Skip invalids
                if (_invalidPlayers.ContainsKey(player)) {
                    continue;
                }

                if (!_detectedPlayers.ContainsKey(player)) {
                    _detectedPlayers[player] = _gameReader.GetPlayerName(player);

                    if (string.IsNullOrEmpty(_detectedPlayers[player])) {
                        Logger.Info($"Player {player} is not valid, not being added to dataset.");
                        _invalidPlayers[player] = true;
                    }
                }
            }

            return _detectedPlayers.Keys
                .Where(p => _invalidPlayers.ContainsKey(p))
                .ToList();
        }

        public bool IsValid(Player player) {
            return _detectedPlayers.ContainsKey(player) && !_invalidPlayers.ContainsKey(player);
        }

        public string GetName(Player player) {
            if (_detectedPlayers.ContainsKey(player))
                return _detectedPlayers[player];

            return string.Empty;
        }


        public bool IsCpu(Player player) {
            var name = GetName(player);
            return name.Contains("CPU") && name.Contains("-");
        }
    }
}