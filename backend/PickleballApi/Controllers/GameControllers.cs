using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PickleballApi.Models;
using PickleballApi.Services;

namespace PickleballApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;

    public GameController(IGameService gameService)
    {
        _gameService = gameService;
    }

    /// <summary>
    /// Get the current game state
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<GameState>> GetCurrentGame()
    {
        var game = await _gameService.GetCurrentGameAsync();

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

        var game = await _gameService.StartNewGameAsync(request.GameType);
        return Ok(game);
        //     return CreatedAtAction(nameof(GetCurrentGame), new { id = game.Id }, game);

    }

    /// <summary>
    /// Update the score for home or away team
    /// </summary>
    [HttpPut("score")]
    public async Task<ActionResult<GameState>> UpdateScore([FromBody] ScoreUpdateRequest request)
    {
        try
        {
            var game = await _gameService.UpdateScoreAsync(request.Team, request.Change);
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
        var stats = await _gameService.GetGameStatsAsync();
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
            await _gameService.ClearStatsAsync();
            var stats = await _gameService.GetGameStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to clear statistics: {ex.Message}");
        }
    }
}
