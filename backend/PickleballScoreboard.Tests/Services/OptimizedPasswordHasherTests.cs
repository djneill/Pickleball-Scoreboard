using FluentAssertions;
using PickleballApi.Models;
using PickleballApi.Services;
using Xunit;
using Microsoft.AspNetCore.Identity;

namespace PickleballScoreboard.Tests.Services;

public class OptimizedPasswordHasherTests
{
    private readonly OptimizedPasswordHasher _hasher;
    private readonly ApplicationUser _testUser;

    public OptimizedPasswordHasherTests()
    {
        _hasher = new OptimizedPasswordHasher();
        _testUser = new ApplicationUser { Id = "test-user-123", UserName = "testuser" };
    }

    [Fact]
    public void HashPassword_CreatesNonEmptyHash()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash = _hasher.HashPassword(_testUser, password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void HashPassword_SamePasswordProducesDifferentHashes()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash1 = _hasher.HashPassword(_testUser, password);
        var hash2 = _hasher.HashPassword(_testUser, password);

        // Assert
        hash1.Should().NotBe(hash2, "salts should be randomly generated");
    }

    [Fact]
    public void VerifyHashedPassword_CorrectPassword_ReturnsSuccess()
    {
        // Arrange
        var password = "TestPassword123!";
        var hash = _hasher.HashPassword(_testUser, password);

        // Act
        var result = _hasher.VerifyHashedPassword(_testUser, hash, password);

        // Assert
        result.Should().Be(PasswordVerificationResult.Success);
    }

    [Fact]
    public void VerifyHashedPassword_IncorrectPassword_ReturnsFailed()
    {
        // Arrange
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword456!";
        var hash = _hasher.HashPassword(_testUser, password);

        // Act
        var result = _hasher.VerifyHashedPassword(_testUser, hash, wrongPassword);

        // Assert
        result.Should().Be(PasswordVerificationResult.Failed);
    }

    [Fact]
    public void VerifyHashedPassword_CaseSensitive()
    {
        // Arrange
        var password = "TestPassword123!";
        var wrongCasePassword = "testpassword123!";
        var hash = _hasher.HashPassword(_testUser, password);

        // Act
        var result = _hasher.VerifyHashedPassword(_testUser, hash, wrongCasePassword);

        // Assert
        result.Should().Be(PasswordVerificationResult.Failed, "passwords should be case-sensitive");
    }

    [Fact]
    public void VerifyHashedPassword_EmptyPassword_ReturnsFailed()
    {
        // Arrange
        var password = "TestPassword123!";
        var hash = _hasher.HashPassword(_testUser, password);

        // Act
        var result = _hasher.VerifyHashedPassword(_testUser, hash, "");

        // Assert
        result.Should().Be(PasswordVerificationResult.Failed);
    }

    [Fact]
    public void VerifyHashedPassword_InvalidHash_ReturnsFailed()
    {
        // Arrange
        var password = "TestPassword123!";
        var invalidHash = "InvalidHashString";

        // Act
        var result = _hasher.VerifyHashedPassword(_testUser, invalidHash, password);

        // Assert
        result.Should().Be(PasswordVerificationResult.Failed);
    }

    [Fact]
    public void VerifyHashedPassword_EmptyHash_ReturnsFailed()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var result = _hasher.VerifyHashedPassword(_testUser, "", password);

        // Assert
        result.Should().Be(PasswordVerificationResult.Failed);
    }

    [Fact]
    public void VerifyHashedPassword_TruncatedHash_ReturnsFailed()
    {
        // Arrange
        var password = "TestPassword123!";
        var hash = _hasher.HashPassword(_testUser, password);
        var truncatedHash = hash.Substring(0, hash.Length / 2);

        // Act
        var result = _hasher.VerifyHashedPassword(_testUser, truncatedHash, password);

        // Assert
        result.Should().Be(PasswordVerificationResult.Failed);
    }

    [Fact]
    public void HashPassword_LongPassword_WorksCorrectly()
    {
        // Arrange
        var longPassword = new string('a', 1000);

        // Act
        var hash = _hasher.HashPassword(_testUser, longPassword);
        var result = _hasher.VerifyHashedPassword(_testUser, hash, longPassword);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        result.Should().Be(PasswordVerificationResult.Success);
    }

    [Fact]
    public void HashPassword_SpecialCharacters_WorksCorrectly()
    {
        // Arrange
        var specialPassword = "!@#$%^&*()_+-=[]{}|;':\",./<>?`~";

        // Act
        var hash = _hasher.HashPassword(_testUser, specialPassword);
        var result = _hasher.VerifyHashedPassword(_testUser, hash, specialPassword);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        result.Should().Be(PasswordVerificationResult.Success);
    }

    [Fact]
    public void HashPassword_UnicodeCharacters_WorksCorrectly()
    {
        // Arrange
        var unicodePassword = "こんにちは世界🔐🌍";

        // Act
        var hash = _hasher.HashPassword(_testUser, unicodePassword);
        var result = _hasher.VerifyHashedPassword(_testUser, hash, unicodePassword);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        result.Should().Be(PasswordVerificationResult.Success);
    }

    [Fact]
    public void VerifyHashedPassword_OneCharacterDifference_ReturnsFailed()
    {
        // Arrange
        var password = "TestPassword123!";
        var similarPassword = "TestPassword123?"; // Only last char different
        var hash = _hasher.HashPassword(_testUser, password);

        // Act
        var result = _hasher.VerifyHashedPassword(_testUser, hash, similarPassword);

        // Assert
        result.Should().Be(PasswordVerificationResult.Failed,
            "even a single character difference should fail verification");
    }

    [Theory]
    [InlineData("password")]
    [InlineData("Password123")]
    [InlineData("P@ssw0rd!")]
    [InlineData("12345678")]
    [InlineData("")]
    [InlineData(" ")]
    public void HashAndVerify_VariousPasswords_WorksCorrectly(string password)
    {
        // Act
        var hash = _hasher.HashPassword(_testUser, password);
        var result = _hasher.VerifyHashedPassword(_testUser, hash, password);

        // Assert
        result.Should().Be(PasswordVerificationResult.Success);
    }

    [Fact]
    public void HashPassword_ProducesBase64EncodedOutput()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash = _hasher.HashPassword(_testUser, password);

        // Assert
        // Valid Base64 should not throw when decoded
        var action = () => Convert.FromBase64String(hash);
        action.Should().NotThrow();
    }

    [Fact]
    public void HashPassword_ProducesExpectedLength()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash = _hasher.HashPassword(_testUser, password);
        var hashBytes = Convert.FromBase64String(hash);

        // Assert
        // Salt (128 bits = 16 bytes) + Hash (256 bits = 32 bytes) = 48 bytes
        hashBytes.Length.Should().Be(48, "hash should contain 16-byte salt + 32-byte hash");
    }
}
