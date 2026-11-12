using ListLift.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ListLift.Maintenance.Commands;

internal sealed class PostgreSqlMigrationCommand(
    ListingDbContext dbContext,
    ILogger<PostgreSqlMigrationCommand> logger)
{
    public async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        if (!string.Equals(dbContext.Database.ProviderName,
                "Npgsql.EntityFrameworkCore.PostgreSQL", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The PostgreSQL migration command requires Database:Provider=PostgreSql.");
        }

        logger.LogInformation("Applying pending ListLift PostgreSQL migrations.");
        await dbContext.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("ListLift PostgreSQL migrations are up to date.");
        return 0;
    }
}
