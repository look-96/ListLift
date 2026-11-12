using ListLift.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ListLift.PostgreSqlMigrations;

public sealed class ListingDbContextFactory : IDesignTimeDbContextFactory<ListingDbContext>
{
    public ListingDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__ListingDb")
            ?? "Host=localhost;Port=5432;Database=listlift;Username=listlift;Password=local-development-only";

        var options = new DbContextOptionsBuilder<ListingDbContext>()
            .UseNpgsql(connectionString, postgres =>
                postgres.MigrationsAssembly("ListLift.PostgreSqlMigrations"))
            .Options;

        return new ListingDbContext(options);
    }
}
