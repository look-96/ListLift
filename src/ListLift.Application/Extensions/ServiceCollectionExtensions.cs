using ListLift.Application.ListingDrafts.Commands;
using ListLift.Application.ListingDrafts.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace ListLift.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddListLiftApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateListingDraftHandler>();
        services.AddScoped<UpdateListingProductFactsHandler>();
        services.AddScoped<SaveApprovedDescriptionHandler>();
        services.AddScoped<GetListingDraftHandler>();
        services.AddScoped<GetActiveListingDraftsHandler>();
        services.AddScoped<DeleteListingDraftHandler>();
        services.AddScoped<UploadListingPhotoHandler>();
        services.AddScoped<UploadListingPhotoValidator>();
        services.AddScoped<ExpireListingDraftsHandler>();
        services.AddScoped<GenerateListingDescriptionHandler>();
        services.AddScoped<SearchMarketplaceCategoriesHandler>();
        services.AddScoped<SelectMarketplaceCategoryHandler>();
        return services;
    }
}
