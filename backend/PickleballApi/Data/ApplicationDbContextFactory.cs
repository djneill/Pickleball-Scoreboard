using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;


namespace PickleballApi.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Use MigrationsConnection for EF migrations (direct connection), fallback to DefaultConnection
        var migrationsConnection = configuration.GetConnectionString("MigrationsConnection");
        var defaultConnection = configuration.GetConnectionString("DefaultConnection");
        var sqliteConnection = configuration.GetConnectionString("SqliteConnection");

        // Check if we have a PostgreSQL connection (contains "Host=" or "Server=")
        var postgresConnection = migrationsConnection ?? defaultConnection;
        if (!string.IsNullOrEmpty(postgresConnection) &&
            (postgresConnection.Contains("Host=") || postgresConnection.Contains("Server=")))
        {
            optionsBuilder.UseNpgsql(postgresConnection);
        }
        else if (!string.IsNullOrEmpty(sqliteConnection))
        {
            optionsBuilder.UseSqlite(sqliteConnection);
        }
        else
        {
            optionsBuilder.UseSqlite("Data Source=pickleball.db");
        }

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
