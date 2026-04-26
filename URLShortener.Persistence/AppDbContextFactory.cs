using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Text.Json;
using URLShortenerAPI.Data;

namespace URLShortener.Persistence;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? "Development";

        string basePath = ResolveConfigurationBasePath();
        string connectionString = GetConnectionString(basePath, environmentName);

        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AppDbContext(options);
    }

    private static string ResolveConfigurationBasePath()
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        string startupProjectDirectory = Path.Combine(currentDirectory, "URLShortenerAPI");

        if (File.Exists(Path.Combine(currentDirectory, "appsettings.json")))
            return currentDirectory;

        if (File.Exists(Path.Combine(startupProjectDirectory, "appsettings.json")))
            return startupProjectDirectory;

        throw new DirectoryNotFoundException(
            "Could not locate appsettings.json for design-time AppDbContext creation.");
    }

    private static string GetConnectionString(string basePath, string environmentName)
    {
        string? connectionString = GetConnectionStringFromFile(Path.Combine(basePath, "appsettings.json"));

        string environmentSettingsPath = Path.Combine(basePath, $"appsettings.{environmentName}.json");
        if (File.Exists(environmentSettingsPath))
            connectionString = GetConnectionStringFromFile(environmentSettingsPath) ?? connectionString;

        connectionString ??= Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        return connectionString
            ?? throw new InvalidOperationException(
                "No database connection string was found at 'ConnectionStrings:DefaultConnection'.");
    }

    private static string? GetConnectionStringFromFile(string path)
    {
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));

        if (!document.RootElement.TryGetProperty("ConnectionStrings", out JsonElement connectionStrings))
            return null;

        if (!connectionStrings.TryGetProperty("DefaultConnection", out JsonElement defaultConnection))
            return null;

        return defaultConnection.GetString();
    }
}
