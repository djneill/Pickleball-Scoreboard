using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PickleballApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Google.Apis.Auth;

namespace PickleballApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        var token = GenerateJwtToken(user);
        return Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email!,
            UserId = user.Id,
            DisplayName = user.DisplayName
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var totalStopwatch = Stopwatch.StartNew();

        // Step 1: Database lookup
        var dbLookupStopwatch = Stopwatch.StartNew();
        var user = await _userManager.FindByEmailAsync(request.Email);
        dbLookupStopwatch.Stop();
        _logger.LogInformation("Login - DB lookup took {ElapsedMs}ms for email {Email}",
            dbLookupStopwatch.ElapsedMilliseconds, request.Email);

        if (user == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        // Step 2: Password verification
        var passwordStopwatch = Stopwatch.StartNew();
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        passwordStopwatch.Stop();
        _logger.LogInformation("Login - Password check took {ElapsedMs}ms for user {UserId}",
            passwordStopwatch.ElapsedMilliseconds, user.Id);

        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        // Step 3: JWT generation
        var jwtStopwatch = Stopwatch.StartNew();
        var token = GenerateJwtToken(user);
        jwtStopwatch.Stop();
        _logger.LogInformation("Login - JWT generation took {ElapsedMs}ms",
            jwtStopwatch.ElapsedMilliseconds);

        totalStopwatch.Stop();
        _logger.LogInformation("Login - TOTAL time: {ElapsedMs}ms (DB: {DbMs}ms, Password: {PwdMs}ms, JWT: {JwtMs}ms)",
            totalStopwatch.ElapsedMilliseconds,
            dbLookupStopwatch.ElapsedMilliseconds,
            passwordStopwatch.ElapsedMilliseconds,
            jwtStopwatch.ElapsedMilliseconds);

        return Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email!,
            UserId = user.Id,
            DisplayName = user.DisplayName
        });
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        var totalStopwatch = Stopwatch.StartNew();

        try
        {
            // Step 1: Verify the Google token
            var googleValidationStopwatch = Stopwatch.StartNew();
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _configuration["Authentication:Google:ClientId"]! }
            };
            var payload = await GoogleJsonWebSignature.ValidateAsync(request.Token, settings);
            googleValidationStopwatch.Stop();
            _logger.LogInformation("Google Login - Token validation took {ElapsedMs}ms",
                googleValidationStopwatch.ElapsedMilliseconds);

            // Step 2: Find or create user
            var dbLookupStopwatch = Stopwatch.StartNew();
            var user = await _userManager.FindByEmailAsync(payload.Email);
            dbLookupStopwatch.Stop();
            _logger.LogInformation("Google Login - DB lookup took {ElapsedMs}ms for email {Email}",
                dbLookupStopwatch.ElapsedMilliseconds, payload.Email);

            long userCreationMs = 0;
            if (user == null)
            {
                // Create new user for Google sign-in
                var userCreationStopwatch = Stopwatch.StartNew();
                user = new ApplicationUser
                {
                    UserName = payload.Email,
                    Email = payload.Email,
                    DisplayName = payload.Name,
                    EmailConfirmed = true // Google emails are pre-verified
                };

                var result = await _userManager.CreateAsync(user);
                userCreationStopwatch.Stop();
                userCreationMs = userCreationStopwatch.ElapsedMilliseconds;
                _logger.LogInformation("Google Login - User creation took {ElapsedMs}ms",
                    userCreationMs);

                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "Failed to create user", errors = result.Errors });
                }
            }

            // Step 3: Generate JWT token
            var jwtStopwatch = Stopwatch.StartNew();
            var token = GenerateJwtToken(user);
            jwtStopwatch.Stop();
            _logger.LogInformation("Google Login - JWT generation took {ElapsedMs}ms",
                jwtStopwatch.ElapsedMilliseconds);

            totalStopwatch.Stop();
            _logger.LogInformation("Google Login - TOTAL time: {ElapsedMs}ms (Google: {GoogleMs}ms, DB: {DbMs}ms, UserCreate: {CreateMs}ms, JWT: {JwtMs}ms)",
                totalStopwatch.ElapsedMilliseconds,
                googleValidationStopwatch.ElapsedMilliseconds,
                dbLookupStopwatch.ElapsedMilliseconds,
                userCreationMs,
                jwtStopwatch.ElapsedMilliseconds);

            return Ok(new AuthResponse
            {
                Token = token,
                Email = user.Email!,
                UserId = user.Id,
                DisplayName = user.DisplayName
            });
        }
        catch (InvalidJwtException)
        {
            return Unauthorized(new { message = "Invalid Google token" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google authentication failed");
            return StatusCode(500, new { message = "Google authentication failed", error = ex.Message });
        }
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("displayName", user.DisplayName ?? string.Empty)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpirationMinutes"]!)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    public string? DisplayName { get; set; }
}

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class GoogleLoginRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
}