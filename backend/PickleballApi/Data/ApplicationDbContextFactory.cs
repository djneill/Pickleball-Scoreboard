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
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        var supabaseConnection = configuration.GetConnectionString("SupabaseConnection");
        var defaultConnection = configuration.GetConnectionString("DefaultConnection");

        if (!string.IsNullOrEmpty(supabaseConnection))
        {
            optionsBuilder.UseNpgsql(supabaseConnection);
        }
        else if (!string.IsNullOrEmpty(defaultConnection))
        {
            optionsBuilder.UseSqlite(defaultConnection);
        }
        else
        {
            // Fallback to SQLite
            optionsBuilder.UseSqlite("Data Source=pickleball.db");
        }

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
