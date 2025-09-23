// Services/GameServiceTests.cs
using PickleballApi.Models;
using PickleballApi.Services;

namespace PickleballScoreboard.Tests.Services;

public class GameServiceTests
{
    private readonly GameService _gameService;

    public GameServiceTests()
    {
        _gameService = new GameService();
    }

    [Fact]
    public async Task GetCurrentGameAsync_WhenNoGameExists_ReturnsNull()
    {
        // Act
        var result = await _gameService.GetCurrentGameAsync();

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(GameType.Singles)]
    [InlineData(GameType.Doubles)]
    public async Task StartNewGameAsync_WithValidGameType_CreatesNewGame(GameType gameType)
    {
        // Act
        var result = await _gameService.StartNewGameAsync(gameType);

        // Assert
        result.Should().NotBeNull();
        result.GameType.Should().Be(gameType);
        result.HomeScore.Should().Be(0);
        result.AwayScore.Should().Be(0);
        result.HomeWins.Should().Be(0);
        result.AwayWins.Should().Be(0);
        result.IsGameComplete.Should().BeFalse();
        result.Id.Should().NotBeEmpty();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task StartNewGameAsync_WhenPreviousGameExists_CompletesOldGame()
    {
        // Arrange
        var firstGame = await _gameService.StartNewGameAsync(GameType.Singles);

        // Act
        var secondGame = await _gameService.StartNewGameAsync(GameType.Doubles);

        // Assert
        firstGame.IsGameComplete.Should().BeTrue();
        firstGame.CompletedAt.Should().NotBeNull();
        secondGame.Should().NotBeSameAs(firstGame);
    }

    [Theory]
    [InlineData("Home", 1, 1, 0)]
    [InlineData("Away", 1, 0, 1)]
    [InlineData("home", 1, 1, 0)] // Test case insensitivity
    [InlineData("AWAY", 1, 0, 1)] // Test case insensitivity
    public async Task UpdateScoreAsync_WithValidInput_UpdatesScore(string team, int change, int expectedHome, int expectedAway)
    {
        // Arrange
        await _gameService.StartNewGameAsync(GameType.Singles);

        // Act
        var result = await _gameService.UpdateScoreAsync(team, change);

        // Assert
        result.HomeScore.Should().Be(expectedHome);
        result.AwayScore.Should().Be(expectedAway);
    }

    [Fact]
    public async Task UpdateScoreAsync_DecrementingScore_PreventsNegativeScores()
    {
        // Arrange
        await _gameService.StartNewGameAsync(GameType.Singles);

        // Act
        var result = await _gameService.UpdateScoreAsync("Home", -1);

        // Assert
        result.HomeScore.Should().Be(0); // Should not go below 0
    }

    [Fact]
    public async Task UpdateScoreAsync_WhenNoActiveGame_ThrowsException()
    {
        // Act & Assert
        var act = async () => await _gameService.UpdateScoreAsync("Home", 1);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No active game found. Start a new game first.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Invalid")]
    public async Task UpdateScoreAsync_WithInvalidTeam_ThrowsArgumentException(string invalidTeam)
    {
        // Arrange
        await _gameService.StartNewGameAsync(GameType.Singles);

        // Act & Assert
        var act = async () => await _gameService.UpdateScoreAsync(invalidTeam, 1);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(-2)]
    public async Task UpdateScoreAsync_WithInvalidChange_ThrowsArgumentException(int invalidChange)
    {
        // Arrange
        await _gameService.StartNewGameAsync(GameType.Singles);

        // Act & Assert
        var act = async () => await _gameService.UpdateScoreAsync("Home", invalidChange);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Score change must be +1 or -1*");
    }

    [Fact]
    public async Task UpdateScoreAsync_WhenGameComplete_ThrowsException()
    {
        // Arrange
        await _gameService.StartNewGameAsync(GameType.Singles);

        // Score to exactly win condition (11-9) - this will complete the game
        await BuildScoresSafely(11, 9);

        // Act & Assert - Try to score after game is complete
        var act = async () => await _gameService.UpdateScoreAsync("Home", 1);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot update score - game is already complete.");
    }

    [Theory]
    [InlineData(11, 9, true, false)]   // Home wins
    [InlineData(9, 11, false, true)]   // Away wins
    [InlineData(10, 8, false, false)]  // No win - under 11
    [InlineData(11, 10, false, false)] // No win - not 2 point lead
    [InlineData(10, 10, false, false)] // No win - tie game
    public async Task UpdateScoreAsync_ChecksWinConditionCorrectly(int homeScore, int awayScore, bool expectedHomeWin, bool expectedAwayWin)
    {
        // Arrange
        await _gameService.StartNewGameAsync(GameType.Singles);

        // Act - Build up scores carefully to avoid premature game completion
        await BuildScoresSafely(homeScore, awayScore);

        var result = await _gameService.GetCurrentGameAsync();

        // Assert
        if (expectedHomeWin || expectedAwayWin)
        {
            result.Should().NotBeNull();
            result!.IsGameComplete.Should().BeTrue();
            result.CompletedAt.Should().NotBeNull();

            if (expectedHomeWin)
            {
                result.HomeWins.Should().Be(1);
                result.AwayWins.Should().Be(0);
            }
            else
            {
                result.HomeWins.Should().Be(0);
                result.AwayWins.Should().Be(1);
            }
        }
        else
        {
            result.Should().NotBeNull();
            result!.IsGameComplete.Should().BeFalse();
            result.CompletedAt.Should().BeNull();
        }
    }

    [Fact]
    public async Task GetGameStatsAsync_WithNoGames_ReturnsEmptyStats()
    {
        // Act
        var result = await _gameService.GetGameStatsAsync();

        // Assert
        result.TotalGamesPlayed.Should().Be(0);
        result.HomeWins.Should().Be(0);
        result.AwayWins.Should().Be(0);
        result.CurrentGame.Should().BeNull();
    }

    [Fact]
    public async Task GetGameStatsAsync_WithCompletedGames_ReturnsCorrectStats()
    {
        // Arrange - Play and complete a few games

        // Game 1: Home wins 11-9
        await _gameService.StartNewGameAsync(GameType.Singles);
        await BuildScoresSafely(11, 9);

        // Game 2: Away wins 11-8  
        await _gameService.StartNewGameAsync(GameType.Doubles);
        await BuildScoresSafely(8, 11);

        // Game 3: Current game in progress
        await _gameService.StartNewGameAsync(GameType.Singles);
        await _gameService.UpdateScoreAsync("Home", 1);

        // Act
        var result = await _gameService.GetGameStatsAsync();

        // Assert
        result.TotalGamesPlayed.Should().Be(2); // Only completed games
        result.HomeWins.Should().Be(1);
        result.AwayWins.Should().Be(1);
        result.CurrentGame.Should().NotBeNull();
        result.CurrentGame!.HomeScore.Should().Be(1);
        result.CurrentGame.AwayScore.Should().Be(0);
    }

    private async Task BuildScoresSafely(int targetHomeScore, int targetAwayScore)
    {
        // Build scores alternately to avoid early game completion
        int homeScore = 0;
        int awayScore = 0;

        while (homeScore < targetHomeScore || awayScore < targetAwayScore)
        {
            // Check if we would trigger a win condition
            if (homeScore < targetHomeScore)
            {
                // Only add home point if it won't complete the game prematurely
                if (!WouldTriggerWin(homeScore + 1, awayScore, targetHomeScore, targetAwayScore))
                {
                    await _gameService.UpdateScoreAsync("Home", 1);
                    homeScore++;
                    continue;
                }
            }

            if (awayScore < targetAwayScore)
            {
                // Only add away point if it won't complete the game prematurely
                if (!WouldTriggerWin(awayScore + 1, homeScore, targetAwayScore, targetHomeScore))
                {
                    await _gameService.UpdateScoreAsync("Away", 1);
                    awayScore++;
                    continue;
                }
            }

            // If we can't add to either without triggering early win, add to the team that needs to reach target
            if (homeScore < targetHomeScore)
            {
                await _gameService.UpdateScoreAsync("Home", 1);
                homeScore++;
            }
            else if (awayScore < targetAwayScore)
            {
                await _gameService.UpdateScoreAsync("Away", 1);
                awayScore++;
            }
            else
            {
                break; // Both scores reached
            }
        }
    }

    private bool WouldTriggerWin(int score, int opponentScore, int targetScore, int targetOpponentScore)
    {
        // Don't trigger win unless we're at the target scores
        if (score != targetScore || opponentScore != targetOpponentScore)
        {
            return score >= 11 && score - opponentScore >= 2;
        }
        return false;
    }

    private async Task PlayGameToScore(int homeScore, int awayScore)
    {
        await BuildScoresSafely(homeScore, awayScore);
    }

    [Fact]
    public async Task ClearStatsAsync_WithExistingGames_ResetsWinCounts()
    {
        // Arrange - Play some games to build up stats

        // Game 1: Home wins 11-9
        await _gameService.StartNewGameAsync(GameType.Singles);
        await BuildScoresSafely(11, 9);

        // Game 2: Away wins 11-8
        await _gameService.StartNewGameAsync(GameType.Doubles);
        await BuildScoresSafely(8, 11);

        // Game 3: Home wins 11-7
        await _gameService.StartNewGameAsync(GameType.Singles);
        await BuildScoresSafely(11, 7);

        // Verify stats before clearing
        var statsBeforeClear = await _gameService.GetGameStatsAsync();
        statsBeforeClear.TotalGamesPlayed.Should().Be(3);
        statsBeforeClear.HomeWins.Should().Be(2);
        statsBeforeClear.AwayWins.Should().Be(1);

        // Act
        await _gameService.ClearStatsAsync();

        // Assert
        var statsAfterClear = await _gameService.GetGameStatsAsync();
        statsAfterClear.TotalGamesPlayed.Should().Be(0);
        statsAfterClear.HomeWins.Should().Be(0);
        statsAfterClear.AwayWins.Should().Be(0);
        statsAfterClear.CurrentGame.Should().BeNull();
    }

    [Fact]
    public async Task ClearStatsAsync_WithNoGames_DoesNotThrow()
    {
        // Act & Assert - Should not throw when no games exist
        var act = async () => await _gameService.ClearStatsAsync();
        await act.Should().NotThrowAsync();

        var stats = await _gameService.GetGameStatsAsync();
        stats.TotalGamesPlayed.Should().Be(0);
        stats.HomeWins.Should().Be(0);
        stats.AwayWins.Should().Be(0);
    }
}