using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using PickleballApi.Data;
using PickleballApi.Models;
using PickleballApi.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("SqliteConnection");
var supabaseConnection = builder.Configuration.GetConnectionString("SupabaseConnection");

if (builder.Environment.EnvironmentName == "Testing")
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("TestDatabase"));
}
else if (builder.Environment.IsProduction())
{
    if (!string.IsNullOrEmpty(supabaseConnection))
    {
        // Production: Use Supabase PostgreSQL
        Console.WriteLine("Using Supabase PostgreSQL database");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(supabaseConnection));
    }
    else
    {
        // Fallback to SQLite if Supabase not configured
        Console.WriteLine("WARNING: Supabase connection not found. Falling back to SQLite.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString ?? "Data Source=pickleball.db"));
    }
}
else
{
    // Development: Use SQLite
    Console.WriteLine("Using SQLite database for development");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString ?? "Data Source=pickleball.db"));
}

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Use optimized password hasher for faster login times (10k iterations vs default 100k)
builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, OptimizedPasswordHasher>();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

// Allow the app to start even if JWT secret is missing (for debugging)
if (string.IsNullOrEmpty(secretKey))
{
    Console.WriteLine("WARNING: JWT SecretKey not configured. Using temporary key.");
    secretKey = "TemporaryKeyForDebugging-ChangeInProduction-AtLeast32Characters!!";
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
if (!string.IsNullOrEmpty(googleClientId))
{
    Console.WriteLine($"Google OAuth configured with ClientId: {googleClientId[..20]}...");
    builder.Services.AddAuthentication().AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
    });
}
else
{
    Console.WriteLine("WARNING: Google OAuth ClientId not configured. Google login will not work.");
}


builder.Services.AddAuthorization();

builder.Services.AddScoped<IGameService, GameService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

    });

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",
            "https://polite-tree-07cef7510.2.azurestaticapps.net",
            "https://pickletrack.onrender.com"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

var app = builder.Build();

// Log startup configuration
Console.WriteLine($"=== Application Starting ===");
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"CORS Origins: localhost:5173, polite-tree-07cef7510.2.azurestaticapps.net, pickletrack.onrender.com");
Console.WriteLine($"========================");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(Options =>
{
    Options.SwaggerEndpoint("/openapi/v1.json", "Pickleball API v1");
});
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
