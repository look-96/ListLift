using ListLift.Application.Abstractions;
using ListLift.Infrastructure.Persistence;
using ListLift.Infrastructure.AI;
using ListLift.Infrastructure.Photos;
using ListLift.Infrastructure.Repositories;
using ListLift.Infrastructure.Time;
using ListLift.Infrastructure.Marketplaces.Ebay;
using ListLift.Infrastructure.GoogleCloud;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ListLift.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddListLiftInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddListLiftDatabase(configuration);
        GoogleCloudStorageRegistration.AddListingStorage(services, configuration);
        services.AddScoped<IListingDraftRepository, ListingDraftRepository>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IListingPhotoProcessor, MagickNetListingPhotoProcessor>();
        services.AddSingleton<IProductDescriptionGenerator, DemoProductDescriptionGenerator>();
        services.AddHttpClient("EbayTaxonomy", client => client.Timeout = TimeSpan.FromSeconds(15));
        services.AddScoped<IEbayApplicationSettingsStore, EbayApplicationSettingsStore>();
        services.AddSingleton<EbayApplicationSettingsProtector>();
        services.AddScoped<IMarketplaceCategoryProvider, EbayTaxonomyCategoryProvider>();
        services.AddHostedService<DatabaseMigrationHostedService>();

        var isCloudRun = !string.IsNullOrWhiteSpace(configuration["K_SERVICE"]) ||
                         !string.IsNullOrWhiteSpace(configuration["K_REVISION"]);
        if (!isCloudRun)
            services.AddHostedService<ListingDraftExpirationHostedService>();

        return services;
    }

    public static IServiceCollection AddListLiftDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ListingDb");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("ConnectionStrings:ListingDb must be configured.");

        var configuredDatabaseProvider = configuration["Database:Provider"];
        var isCloudRun = !string.IsNullOrWhiteSpace(configuration["K_SERVICE"]) ||
                         !string.IsNullOrWhiteSpace(configuration["K_REVISION"]);
        if (isCloudRun &&
            !string.Equals(configuredDatabaseProvider, "PostgreSql", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Database:Provider must be PostgreSql when hosted on Cloud Run.");
        }

        var databaseProvider = configuredDatabaseProvider ?? "SqlServer";
        services.AddDbContext<ListingDbContext>(options =>
        {
            if (string.Equals(databaseProvider, "PostgreSql", StringComparison.OrdinalIgnoreCase))
            {
                options.UseNpgsql(connectionString, postgres =>
                    postgres.MigrationsAssembly("ListLift.PostgreSqlMigrations"));
                return;
            }

            if (string.Equals(databaseProvider, "SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlServer(connectionString);
                return;
            }

            throw new InvalidOperationException("Database:Provider must be SqlServer or PostgreSql.");
        });
        return services;
    }
}
