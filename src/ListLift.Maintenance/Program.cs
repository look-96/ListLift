using ListLift.Application.Extensions;
using ListLift.Infrastructure.Extensions;
using ListLift.Maintenance.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

if (args.Length != 1 || args[0] is not ("migrate-postgresql" or "cleanup-expired-drafts"))
{
    Console.Error.WriteLine("Usage: ListLift.Maintenance <migrate-postgresql|cleanup-expired-drafts>");
    return 2;
}

var builder = Host.CreateApplicationBuilder();
if (args[0] == "migrate-postgresql")
{
    builder.Services.AddListLiftDatabase(builder.Configuration);
    builder.Services.AddScoped<PostgreSqlMigrationCommand>();
}
else
{
    builder.Services.AddListLiftApplication();
    builder.Services.AddListLiftInfrastructure(builder.Configuration);
    builder.Services.AddScoped<ExpiredDraftCleanupCommand>();
}

using var host = builder.Build();
var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("ListLift.Maintenance");

try
{
    await using var scope = host.Services.CreateAsyncScope();
    return args[0] switch
    {
        "migrate-postgresql" => await scope.ServiceProvider
            .GetRequiredService<PostgreSqlMigrationCommand>()
            .RunAsync(CancellationToken.None),
        "cleanup-expired-drafts" => await scope.ServiceProvider
            .GetRequiredService<ExpiredDraftCleanupCommand>()
            .RunAsync(CancellationToken.None),
        _ => 2
    };
}
catch (Exception exception)
{
    logger.LogCritical(exception, "ListLift maintenance command {Command} failed.", args[0]);
    return 1;
}
