using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PickleballApi.Models;
using PickleballApi.Services;

namespace PickleballApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;

    public GameController(IGameService gameService)
    {
        _gameService = gameService;
    }

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User ID not found in token");
    }

    /// <summary>
    /// Get the current game state
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<GameState>> GetCurrentGame()
    {
        var userId = GetCurrentUserId();
        var game = await _gameService.GetCurrentGameAsync(userId);

        if (game == null)
        {
            return NotFound("No active game found.");
        }

        return Ok(game);
    }

    /// <summary>
    /// Start a new game
    /// </summary>
    [HttpPost("new")]
    public async Task<ActionResult<GameState>> StartNewGame([FromBody] NewGameRequest request)
    {
        if (request == null)
        {
            return BadRequest("Request body is required");
        }

        var userId = GetCurrentUserId();
        var game = await _gameService.StartNewGameAsync(userId, request.GameType);
        return Ok(game);
    }

    /// <summary>
    /// Update the score for home or away team
    /// </summary>
    [HttpPut("score")]
    public async Task<ActionResult<GameState>> UpdateScore([FromBody] ScoreUpdateRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var game = await _gameService.UpdateScoreAsync(userId, request.Team, request.Change);
            return Ok(game);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get match statistics and game history
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<GameStatsResponse>> GetGameStats()
    {
        var userId = GetCurrentUserId();
        var stats = await _gameService.GetGameStatsAsync(userId);
        return Ok(stats);
    }

    /// <summary>
    /// Clear all match stats
    /// </summary>
    [HttpDelete("stats")]
    public async Task<ActionResult<GameStatsResponse>> ClearStats()
    {
        try
        {
            var userId = GetCurrentUserId();
            await _gameService.ClearStatsAsync(userId);
            var stats = await _gameService.GetGameStatsAsync(userId);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to clear statistics: {ex.Message}");
        }
    }
}
