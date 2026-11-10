using ListLift.Application.Abstractions;
using ListLift.Application.ListingDrafts.Exceptions;
using ListLift.Application.Validation;
using ListLift.Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace ListLift.Application.ListingDrafts.Commands;

public sealed class SearchMarketplaceCategoriesHandler(
    IEnumerable<IMarketplaceCategoryProvider> providers)
{
    public async Task<IReadOnlyList<MarketplaceCategorySuggestion>> HandleAsync(
        SearchMarketplaceCategoriesQuery query,
        CancellationToken cancellationToken)
    {
        DataAnnotationsRequestValidator.Validate(query);
        if (!Enum.IsDefined(query.Channel))
            throw new ArgumentOutOfRangeException(nameof(query), "Unknown marketplace.");

        var provider = providers.SingleOrDefault(candidate => candidate.Channel == query.Channel);
        if (provider is null)
            throw new MarketplaceCategoryProviderUnavailableException();

        var trimmedQuery = query.Query.Trim();
        if (trimmedQuery.Length < 2)
            throw new ValidationException("Enter at least two characters to search categories.");

        return await provider.SearchAsync(trimmedQuery, cancellationToken);
    }
}
