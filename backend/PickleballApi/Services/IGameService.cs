using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PickleballApi.Models;

namespace PickleballApi.Services;
/// <summary>
/// Service for managing pickleball game logic and state
/// </summary>

public interface IGameService
{
    /// <summary>
    /// Get the current active game
    /// </summary
    Task<GameState?> GetCurrentGameAsync(string userId);

    /// <summary>
    /// Start a new game with the specified type
    /// </summary
    /// <param name="gameType">Singles or Doubles</param>
    /// <returns>The newly created game</returns>
    Task<GameState> StartNewGameAsync(string userId, GameType gametype);

    /// <summary>
    /// Update the score for a team
    /// </summary>
    /// <param name="team">Team name ("Home" or "Away")</param>
    /// <param name="change">Score change (+1 or -1)</param>
    /// <returns>Updated game state</returns>
    /// <exception cref="ArgumentException">Thrown when team name is invalid</exception>
    /// <exception cref="InvalidOperationException">Thrown when no active game or game is complete</exception>
    Task<GameState> UpdateScoreAsync(string userId, string team, int change);

    /// <summary>
    /// Get overall game statistics including win counts
    /// </summary>
    Task<GameStatsResponse> GetGameStatsAsync(string userId);

    /// <summary>
    /// Clear all match statistics and reset win counts
    /// </summary>
    Task ClearStatsAsync(string userId);
}
