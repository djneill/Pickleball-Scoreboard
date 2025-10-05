using Microsoft.EntityFrameworkCore;
using PickleballApi.Data;
using PickleballApi.Models;

namespace PickleballApi.Services;

public class GameService : IGameService
{
    private readonly ApplicationDbContext _context;

    public GameService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GameState?> GetCurrentGameAsync(string userId)
    {
        var currentGame = await _context.Games
            .Where(g => g.UserId == userId && !g.IsComplete)
            .OrderByDescending(g => g.CreatedAt)
            .FirstOrDefaultAsync();

        if (currentGame == null)
        {
            return null;
        }

        // Get stats for this user
        var stats = await GetStatsForUser(userId);

        return new GameState
        {
            GameType = currentGame.GameType,
            HomeScore = currentGame.HomeScore,
            AwayScore = currentGame.AwayScore,
            HomeWins = stats.HomeWins,
            AwayWins = stats.AwayWins,
            IsGameComplete = currentGame.IsComplete,
            CompletedAt = currentGame.CompletedAt
        };
    }

    public async Task<GameState> StartNewGameAsync(string userId, GameType gameType)
    {
        // Mark any existing incomplete game as complete
        var existingGame = await _context.Games
            .Where(g => g.UserId == userId && !g.IsComplete)
            .FirstOrDefaultAsync();

        if (existingGame != null)
        {
            existingGame.IsComplete = true;
            existingGame.CompletedAt = DateTime.UtcNow;
        }

        // Get current stats
        var stats = await GetStatsForUser(userId);

        // Create new game
        var newGame = new Game
        {
            UserId = userId,
            GameType = gameType,
            HomeScore = 0,
            AwayScore = 0,
            IsComplete = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Games.Add(newGame);
        await _context.SaveChangesAsync();

        return new GameState
        {
            GameType = newGame.GameType,
            HomeScore = newGame.HomeScore,
            AwayScore = newGame.AwayScore,
            HomeWins = stats.HomeWins,
            AwayWins = stats.AwayWins,
            IsGameComplete = false
        };
    }

    public async Task<GameState> UpdateScoreAsync(string userId, string team, int change)
    {
        var currentGame = await _context.Games
            .Where(g => g.UserId == userId && !g.IsComplete)
            .OrderByDescending(g => g.CreatedAt)
            .FirstOrDefaultAsync();

        if (currentGame == null)
        {
            throw new InvalidOperationException("No active game found. Start a new game first.");
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
                currentGame.HomeScore = Math.Max(0, currentGame.HomeScore + change);
                break;
            case "away":
                currentGame.AwayScore = Math.Max(0, currentGame.AwayScore + change);
                break;
            default:
                throw new ArgumentException($"Invalid team name: {team}. Must be 'Home' or 'Away'", nameof(team));
        }

        // Check for win condition
        await CheckForWinCondition(userId, currentGame);

        await _context.SaveChangesAsync();

        // Get updated stats
        var stats = await GetStatsForUser(userId);

        return new GameState
        {
            GameType = currentGame.GameType,
            HomeScore = currentGame.HomeScore,
            AwayScore = currentGame.AwayScore,
            HomeWins = stats.HomeWins,
            AwayWins = stats.AwayWins,
            IsGameComplete = currentGame.IsComplete,
            CompletedAt = currentGame.CompletedAt
        };
    }

    public async Task<GameStatsResponse> GetGameStatsAsync(string userId)
    {
        var stats = await GetStatsForUser(userId);
        var currentGame = await GetCurrentGameAsync(userId);

        return new GameStatsResponse
        {
            TotalGamesPlayed = stats.HomeWins + stats.AwayWins,
            HomeWins = stats.HomeWins,
            AwayWins = stats.AwayWins,
            CurrentGame = currentGame
        };
    }

    public async Task ClearStatsAsync(string userId)
    {
        // Delete all games for this user
        var userGames = await _context.Games
            .Where(g => g.UserId == userId)
            .ToListAsync();

        _context.Games.RemoveRange(userGames);

        // Delete statistics for this user
        var userStats = await _context.MatchStatistics
            .Where(s => s.UserId == userId)
            .ToListAsync();

        _context.MatchStatistics.RemoveRange(userStats);

        await _context.SaveChangesAsync();
    }

    private async Task CheckForWinCondition(string userId, Game currentGame)
    {
        const int winningScore = 11;
        const int minLeadToWin = 2;

        var homeScore = currentGame.HomeScore;
        var awayScore = currentGame.AwayScore;

        bool homeWins = homeScore >= winningScore && homeScore - awayScore >= minLeadToWin;
        bool awayWins = awayScore >= winningScore && awayScore - homeScore >= minLeadToWin;

        if (homeWins || awayWins)
        {
            currentGame.IsComplete = true;
            currentGame.CompletedAt = DateTime.UtcNow;

            // Update or create statistics
            var stats = await _context.MatchStatistics
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (stats == null)
            {
                stats = new MatchStatistics
                {
                    UserId = userId,
                    HomeWins = 0,
                    AwayWins = 0
                };
                _context.MatchStatistics.Add(stats);
            }

            if (homeWins)
            {
                stats.HomeWins++;
            }
            else
            {
                stats.AwayWins++;
            }

            stats.LastUpdated = DateTime.UtcNow;
        }
    }

    private async Task<(int HomeWins, int AwayWins)> GetStatsForUser(string userId)
    {
        var stats = await _context.MatchStatistics
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (stats == null)
        {
            return (0, 0);
        }

        return (stats.HomeWins, stats.AwayWins);
    }
}