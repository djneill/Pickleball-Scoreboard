using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using PickleballApi.Controllers;
using PickleballApi.Data;
using PickleballApi.Models;
using PickleballApi.Services;
using System.Security.Claims;
using Xunit;

namespace PickleballScoreboard.Tests;

public class GameControllerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly GameService _gameService;
    private readonly GameController _controller;
    private readonly string _testUserId = "test-user-123";

    public GameControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _gameService = new GameService(_context);
        _controller = new GameController(_gameService);

        // Mock the authenticated user
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, _testUserId) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetCurrentGame_NoActiveGame_ReturnsNotFound()
    {
        var result = await _controller.GetCurrentGame();
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task StartNewGame_CreatesGame_ReturnsOk()
    {
        var request = new NewGameRequest { GameType = GameType.Singles };
        var result = await _controller.StartNewGame(request);
        result.Result.Should().BeOfType<OkObjectResult>();
    }
}