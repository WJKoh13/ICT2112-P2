using ProRental.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ProRental;

/// <summary>
/// Allows the EF Core CLI tools (dotnet ef) to create an AppDbContext
/// without needing a running ASP.NET application.
/// Reads the connection string from appsettings.json / appsettings.Development.json.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Walk up from the current directory to find the folder containing appsettings.json.
        // This handles running 'dotnet ef' from either the project folder or the solution root.
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "appsettings.json")))
            dir = dir.Parent;
        var basePath = dir?.FullName ?? Directory.GetCurrentDirectory();

        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = config.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Connection string 'Default' not found. " +
                "Ensure appsettings.Development.json exists with a valid PostgreSQL connection string.");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
