using Microsoft.AspNetCore.Mvc;
using PickleballApi.Controllers;
using PickleballApi.Models;
using PickleballApi.Services;
using FluentAssertions;

namespace PickleballScoreboard.Tests.Controllers;

public class GameControllerTests
{
    private readonly GameController _controller;
    private readonly IGameService _gameService;

    public GameControllerTests()
    {
        _gameService = new GameService();
        _controller = new GameController(_gameService);
    }

    [Fact]
    public async Task ClearStats_ReturnsOkWithEmptyStats()
    {
        // Arrange - Create some games first
        await _gameService.StartNewGameAsync(GameType.Singles);
        await _gameService.UpdateScoreAsync("Home", 1);

        // Act
        var result = await _controller.ClearStats();

        // Assert
        result.Should().BeOfType<ActionResult<GameStatsResponse>>();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var stats = okResult.Value.Should().BeOfType<GameStatsResponse>().Subject;

        stats.TotalGamesPlayed.Should().Be(0);
        stats.HomeWins.Should().Be(0);
        stats.AwayWins.Should().Be(0);
    }
}