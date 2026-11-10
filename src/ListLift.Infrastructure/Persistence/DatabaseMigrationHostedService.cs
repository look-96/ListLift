using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace ListLift.Infrastructure.Persistence;

internal sealed class DatabaseMigrationHostedService(
    IServiceScopeFactory scopeFactory,
    IHostEnvironment environment,
    ILogger<DatabaseMigrationHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            logger.LogInformation("Automatic database migrations are disabled outside Development.");
            return;
        }

        logger.LogInformation("Applying pending ListLift database migrations for Development.");
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ListingDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("ListLift database migrations are up to date.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
