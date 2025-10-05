using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PickleballApi.Data;
using PickleballApi.Models;
using PickleballApi.Services;
using Xunit;

namespace PickleballScoreboard.Tests;

public class GameServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly GameService _gameService;
    private readonly string _testUserId = "test-user-123";
    private readonly string _otherUserId = "other-user-456";

    public GameServiceTests()
    {
        // Create in-memory database for each test
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _gameService = new GameService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetCurrentGameAsync_NoActiveGame_ReturnsNull()
    {
        // Act
        var result = await _gameService.GetCurrentGameAsync(_testUserId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task StartNewGameAsync_CreatesNewGame()
    {
        // Act
        var result = await _gameService.StartNewGameAsync(_testUserId, GameType.Singles);

        // Assert
        result.Should().NotBeNull();
        result.GameType.Should().Be(GameType.Singles);
        result.HomeScore.Should().Be(0);
        result.AwayScore.Should().Be(0);
        result.IsGameComplete.Should().BeFalse();
    }

    [Fact]
    public async Task StartNewGameAsync_WithExistingIncompleteGame_CompletesOldGame()
    {
        // Arrange
        await _gameService.StartNewGameAsync(_testUserId, GameType.Singles);

        // Act
        await _gameService.StartNewGameAsync(_testUserId, GameType.Doubles);

        // Assert
        var completedGames = await _context.Games
            .Where(g => g.UserId == _testUserId && g.IsComplete)
            .ToListAsync();

        completedGames.Should().HaveCount(1);
        completedGames[0].GameType.Should().Be(GameType.Singles);
    }

    [Fact]
    public async Task UpdateScoreAsync_NoActiveGame_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _gameService.UpdateScoreAsync(_testUserId, "home", 1));
    }

    [Fact]
    public async Task UpdateScoreAsync_IncreasesHomeScore()
    {
        // Arrange
        await _gameService.StartNewGameAsync(_testUserId, GameType.Singles);

        // Act
        var result = await _gameService.UpdateScoreAsync(_testUserId, "home", 1);

        // Assert
        result.HomeScore.Should().Be(1);
        result.AwayScore.Should().Be(0);
    }

    [Fact]
    public async Task UpdateScoreAsync_IncreasesAwayScore()
    {
        // Arrange
        await _gameService.StartNewGameAsync(_testUserId, GameType.Singles);

        // Act
        var result = await _gameService.UpdateScoreAsync(_testUserId, "away", 1);

        // Assert
        result.HomeScore.Should().Be(0);
        result.AwayScore.Should().Be(1);
    }

    [Fact]
    public async Task UpdateScoreAsync_DecreasesScore()
    {
        // Arrange
        await _gameService.StartNewGameAsync(_testUserId, GameType.Singles);
        await _gameService.UpdateScoreAsync(_testUserId, "home", 1);
        await _gameService.UpdateScoreAsync(_testUserId, "home", 1);

        // Act
        var result = await _gameService.UpdateScoreAsync(_testUserId, "home", -1);

        // Assert
        result.HomeScore.Should().Be(1);
    }

    [Fact]
    public async Task UpdateScoreAsync_CannotGoNegative()
    {
        // Arrange
        await _gameService.StartNewGameAsync(_testUserId, GameType.Singles);

        // Act
        var result = await _gameService.UpdateScoreAsync(_testUserId, "home", -1);

        // Assert
        result.HomeScore.Should().Be(0);
    }

    [Fact]
    public async Task UpdateScoreAsync_InvalidTeam_ThrowsException()
    {
        // Arrange
        await _gameService.StartNewGameAsync(_testUserId, GameType.Singles);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _gameService.UpdateScoreAsync(_testUserId, "invalid", 1));
    }

    [Fact]
    public async Task UpdateScoreAsync_InvalidChange_ThrowsException()
    {
        // Arrange
        await _gameService.StartNewGameAsync(_testUserId, GameType.Singles);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _gameService.UpdateScoreAsync(_testUserId, "home", 5));
    }

    [Fact]
    public async Task UpdateScoreAsync_EmptyTeam_ThrowsException()
    {
        // Arrange
        await _gameService.StartNewGameAsync(_testUserId, GameType.Singles);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _gameService.UpdateScoreAsync(_testUserId, "", 1));
    }

    [Fact]
    public async Task UpdateScoreAsync_HomeWinsAt11With2PointLead()
    {
        // Arrange
        await _gameService.StartNewGameAsync(_testUserId, GameType.Singles);

        // Score to 10-9 first
        for (int i = 0; i < 10; i++)
            await _gameService.UpdateScoreAsync(_testUserId, "home", 1);
        for (int i = 0; i < 9; i++)
            await _gameService.UpdateScoreAsync(_testUserId, "away", 1);

        // Act - Score home to 11 (wins by 2)
        var result = await _gameService.UpdateScoreAsync(_testUserId, "home", 1);

        // Assert
        result.IsGameComplete.Should().BeTrue();
        result.HomeScore.Should().Be(11);
        result.AwayScore.Should().Be(9);
        result.HomeWins.Should().Be(1);
    }

    [Fact]
    public async Task UpdateScoreAsync_MustWinBy2()
    {
        // Arrange
        await _gameService.StartNewGameAsync(_testUserId, GameType.Singles);

        // Score to 10-10 first
        for (int i = 0; i < 10; i++)
            await _gameService.UpdateScoreAsync(_testUserId, "home", 1);
        for (int i = 0; i < 10; i++)
            await _gameService.UpdateScoreAsync(_testUserId, "away", 1);

        // Act - Score home to 11-10 (not enough to win)
        var result = await _gameService.UpdateScoreAsync(_testUserId, "home", 1);

        // Assert - Game continues, need 2-point lead
        result.IsGameComplete.Should().BeFalse();
        result.HomeScore.Should().Be(11);
        result.AwayScore.Should().Be(10);
    }

    [Fact]
    public async Task GetGameStatsAsync_NoGames_ReturnsZeroStats()
    {
        // Act
        var result = await _gameService.GetGameStatsAsync(_testUserId);

        // Assert
        result.TotalGamesPlayed.Should().Be(0);
        result.HomeWins.Should().Be(0);
        result.AwayWins.Should().Be(0);
        result.CurrentGame.Should().BeNull();
    }

    [Fact]
    public async Task GetGameStatsAsync_WithCompletedGames_ReturnsCorrectStats()
    {
        // Arrange - Complete a home win
        await _gameService.StartNewGameAsync(_testUserId, GameType.Singles);
        for (int i = 0; i < 11; i++)
            await _gameService.UpdateScoreAsync(_testUserId, "home", 1);

        // Complete an away win
        await _gameService.StartNewGameAsync(_testUserId, GameType.Singles);
        for (int i = 0; i < 11; i++)
            await _gameService.UpdateScoreAsync(_testUserId, "away", 1);

        // Act
        var result = await _gameService.GetGameStatsAsync(_testUserId);

        // Assert
        result.TotalGamesPlayed.Should().Be(2);
        result.HomeWins.Should().Be(1);
        result.AwayWins.Should().Be(1);
    }

    [Fact]
    public async Task ClearStatsAsync_RemovesAllGamesAndStats()
    {
        // Arrange
        await _gameService.StartNewGameAsync(_testUserId, GameType.Singles);
        for (int i = 0; i < 11; i++)
            await _gameService.UpdateScoreAsync(_testUserId, "home", 1);

        // Act
        await _gameService.ClearStatsAsync(_testUserId);

        // Assert
        var stats = await _gameService.GetGameStatsAsync(_testUserId);
        stats.TotalGamesPlayed.Should().Be(0);
        stats.HomeWins.Should().Be(0);
        stats.AwayWins.Should().Be(0);

        var currentGame = await _gameService.GetCurrentGameAsync(_testUserId);
        currentGame.Should().BeNull();
    }

    [Fact]
    public async Task UserIsolation_UsersCannotSeeEachOthersGames()
    {
        // Arrange
        await _gameService.StartNewGameAsync(_testUserId, GameType.Singles);
        await _gameService.UpdateScoreAsync(_testUserId, "home", 1);

        await _gameService.StartNewGameAsync(_otherUserId, GameType.Doubles);
        await _gameService.UpdateScoreAsync(_otherUserId, "away", 1);

        // Act
        var user1Game = await _gameService.GetCurrentGameAsync(_testUserId);
        var user2Game = await _gameService.GetCurrentGameAsync(_otherUserId);

        // Assert
        user1Game.Should().NotBeNull();
        user1Game!.GameType.Should().Be(GameType.Singles);
        user1Game.HomeScore.Should().Be(1);
        user1Game.AwayScore.Should().Be(0);

        user2Game.Should().NotBeNull();
        user2Game!.GameType.Should().Be(GameType.Doubles);
        user2Game.HomeScore.Should().Be(0);
        user2Game.AwayScore.Should().Be(1);
    }

    [Fact]
    public async Task UserIsolation_ClearStatsOnlyAffectsCurrentUser()
    {
        // Arrange
        await _gameService.StartNewGameAsync(_testUserId, GameType.Singles);
        await _gameService.StartNewGameAsync(_otherUserId, GameType.Doubles);

        // Act
        await _gameService.ClearStatsAsync(_testUserId);

        // Assert
        var user1Game = await _gameService.GetCurrentGameAsync(_testUserId);
        var user2Game = await _gameService.GetCurrentGameAsync(_otherUserId);

        user1Game.Should().BeNull();
        user2Game.Should().NotBeNull();
    }
}