using ListLift.Domain.Constants;

namespace ListLift.Application.Abstractions;

public interface IMarketplaceCategoryProvider
{
    MarketplaceChannel Channel { get; }

    Task<IReadOnlyList<MarketplaceCategorySuggestion>> SearchAsync(
        string query,
        CancellationToken cancellationToken);
}
