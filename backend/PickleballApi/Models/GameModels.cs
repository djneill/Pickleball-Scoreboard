using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PickleballApi.Models
{
    public class GameState
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public GameType GameType { get; set; }
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
        public int HomeWins { get; set; }
        public int AwayWins { get; set; }
        public bool IsGameComplete { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }

    public enum GameType
    {
        Singles,
        Doubles
    }

    public class NewGameRequest
    {
        public GameType GameType { get; set; }
    }

    public class ScoreUpdateRequest
    {
        public string Team { get; set; } = string.Empty;
        public int Change { get; set; }
    }

    public class GameStatsResponse
    {
        public int TotalGamesPlayed { get; set; }
        public int HomeWins { get; set; }
        public int AwayWins { get; set; }
        public GameState? CurrentGame { get; set; }
    }
}