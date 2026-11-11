using ListLift.Application.Abstractions;
using ListLift.Infrastructure.Persistence;
using ListLift.Infrastructure.AI;
using ListLift.Infrastructure.Photos;
using ListLift.Infrastructure.Azure;
using ListLift.Infrastructure.Repositories;
using ListLift.Infrastructure.Time;
using ListLift.Infrastructure.Marketplaces.Ebay;
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
        var connectionString = configuration.GetConnectionString("ListingDb");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("ConnectionStrings:ListingDb must be configured.");

        services.AddDbContext<ListingDbContext>(options => options.UseSqlServer(connectionString));
        AzureStorageRegistration.AddListingStorage(services, configuration);
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
        services.AddHostedService<ListingDraftExpirationHostedService>();
        return services;
    }
}
