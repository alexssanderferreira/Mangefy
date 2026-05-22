using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Mangefy.Infrastructure.Persistence;

/// <summary>
/// Usado pelo CLI do EF Core (dotnet ef migrations add / update) em design-time.
/// </summary>
public sealed class MangefyDbContextFactory : IDesignTimeDbContextFactory<MangefyDbContext>
{
    public MangefyDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Mangefy.API"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<MangefyDbContext>();
        optionsBuilder.UseNpgsql(config.GetConnectionString("DefaultConnection"));

        return new MangefyDbContext(optionsBuilder.Options);
    }
}
