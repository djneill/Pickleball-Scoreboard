using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PickleballApi.Models;

namespace PickleballApi.Services
{
    public class GameService : IGameService
    {
        private GameState? _currentGame;
        private readonly List<GameState> _gameHistory = new();

        public Task<GameState?> GetCurrentGameAsync()
        {
            return Task.FromResult(_currentGame);
        }

        public Task<GameState> StartNewGameAsync(GameType gametype)
        {
            if (_currentGame != null && !_currentGame.IsGameComplete)
            {
                _currentGame.IsGameComplete = true;
                _currentGame.CompletedAt = DateTime.UtcNow;
            }

            _currentGame = new GameState
            {
                GameType = gametype,
                HomeScore = 0,
                AwayScore = 0,
                HomeWins = _gameHistory.Count(g => g.HomeScore > g.AwayScore),
                AwayWins = _gameHistory.Count(g => g.AwayScore > g.HomeScore),
                IsGameComplete = false
            };
            return Task.FromResult(_currentGame);
        }

        public Task<GameState> UpdateScoreAsync(string team, int change)
        {
            if (_currentGame == null)
            {
                throw new InvalidOperationException("No active game found. Start a new game first.");
            }
            if (_currentGame.IsGameComplete)
            {
                throw new InvalidOperationException("Cannot update score - game is already complete.");
            }
            if (string.IsNullOrWhiteSpace(team))
            {
                throw new ArgumentException("Team cannot be empty", nameof(team));
            }
            if (change != 1 && change != -1)
            {
                throw new ArgumentException("Score change must be +1 or -1", nameof(change));
            }

            switch (team.ToLowerInvariant())
            {
                case "home":
                    _currentGame.HomeScore = Math.Max(0, _currentGame.HomeScore + change);
                    break;
                case "away":
                    _currentGame.AwayScore = Math.Max(0, _currentGame.AwayScore + change);
                    break;
                default:
                    throw new ArgumentException($"Invalid team name: {team}. Must be 'Home' or 'Away'", nameof(team));
            }

            CheckforWinCondition();

            return Task.FromResult(_currentGame);
        }

        public Task<GameStatsResponse> GetGameStatsAsync()
        {
            var completedGames = _gameHistory.Where(g => g.IsGameComplete).ToList();

            var stats = new GameStatsResponse
            {
                TotalGamesPlayed = completedGames.Count,
                HomeWins = completedGames.Count(g => g.HomeScore > g.AwayScore),
                AwayWins = completedGames.Count(g => g.AwayScore > g.HomeScore),
                CurrentGame = _currentGame
            };

            return Task.FromResult(stats);
        }

        private void CheckforWinCondition()
        {
            if (_currentGame == null) return;

            const int winningScore = 11;
            const int minLeadToWin = 2;

            var homeScore = _currentGame.HomeScore;
            var awayScore = _currentGame.AwayScore;

            bool homeWins = homeScore >= winningScore && homeScore - awayScore >= minLeadToWin;
            bool awayWins = awayScore >= winningScore && awayScore - homeScore >= minLeadToWin;

            if (homeWins || awayWins)
            {
                _currentGame.IsGameComplete = true;
                _currentGame.CompletedAt = DateTime.UtcNow;

                if (homeWins)
                {
                    _currentGame.HomeWins++;
                }
                else
                {
                    _currentGame.AwayWins++;
                }
                _gameHistory.Add(_currentGame);
            }
        }
        public Task ClearStatsAsync()
        {
            // Clear the game history (which tracks wins)
            _gameHistory.Clear();

            // Reset current game win counts if there's an active game
            if (_currentGame != null)
            {
                _currentGame.HomeWins = 0;
                _currentGame.AwayWins = 0;
            }

            return Task.CompletedTask;
        }
    }
}