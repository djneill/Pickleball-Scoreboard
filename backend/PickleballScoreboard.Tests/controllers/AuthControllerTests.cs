using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PickleballApi.Controllers;
using PickleballApi.Data;
using PickleballApi.Models;
using Xunit;

namespace PickleballScoreboard.Tests;

public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsToken()
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "test@example.com",
            Password: "Test123!",
            DisplayName: "Test User"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
        authResponse.Email.Should().Be("test@example.com");
        authResponse.DisplayName.Should().Be("Test User");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "duplicate@example.com",
            Password: "Test123!",
            DisplayName: "First User"
        );

        // Register first user
        await _client.PostAsJsonAsync("/api/auth/register", request);

        // Try to register again with same email
        var duplicateRequest = new RegisterRequest(
            Email: "duplicate@example.com",
            Password: "Test123!",
            DisplayName: "Second User"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", duplicateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "weak@example.com",
            Password: "weak",  // Too short, no uppercase, no digit
            DisplayName: "Weak Password User"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Password");
    }

    [Fact]
    public async Task Register_WithMissingEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "",
            Password: "Test123!",
            DisplayName: "No Email User"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange - Register a user first
        var registerRequest = new RegisterRequest(
            Email: "login@example.com",
            Password: "Test123!",
            DisplayName: "Login User"
        );
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest(
            Email: "login@example.com",
            Password: "Test123!"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
        authResponse.Email.Should().Be("login@example.com");
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        // Arrange - Register a user first
        var registerRequest = new RegisterRequest(
            Email: "wrongpass@example.com",
            Password: "Test123!",
            DisplayName: "User"
        );
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest(
            Email: "wrongpass@example.com",
            Password: "WrongPassword123!"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest(
            Email: "nonexistent@example.com",
            Password: "Test123!"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_TokenContainsCorrectClaims()
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "claims@example.com",
            Password: "Test123!",
            DisplayName: "Claims User"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

        // Assert - Decode the JWT token
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(authResponse!.Token);

        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "claims@example.com");
        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub);
        token.Claims.Should().Contain(c => c.Type == "displayName" && c.Value == "Claims User");
    }

    [Fact]
    public async Task Register_WithNullDisplayName_Succeeds()
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "nodisplay@example.com",
            Password: "Test123!",
            DisplayName: null
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.DisplayName.Should().BeNull();
    }

    [Fact]
    public async Task Login_TokenCanBeUsedForAuthentication()
    {
        // Arrange - Register and login to get token
        var registerRequest = new RegisterRequest(
            Email: "authtest@example.com",
            Password: "Test123!",
            DisplayName: "Auth Test User"
        );
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest(
            Email: "authtest@example.com",
            Password: "Test123!"
        );
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        // Act - Try to access a protected endpoint with the token
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse!.Token);

        var protectedResponse = await _client.GetAsync("/api/game");

        // Assert - Should not get 401 Unauthorized
        protectedResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }
}

// Helper classes for deserialization (if not already in your project)
public record AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
}

public record RegisterRequest(string Email, string Password, string? DisplayName);
public record LoginRequest(string Email, string Password);