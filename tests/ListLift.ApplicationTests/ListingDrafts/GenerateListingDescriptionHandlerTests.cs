using ListLift.Application.Abstractions;
using ListLift.Application.ListingDrafts.Commands;
using ListLift.Application.ListingDrafts.Exceptions;
using ListLift.ApplicationTests.TestDoubles;
using ListLift.Domain.Entities;
using ListLift.Domain.ValueObjects;
using Xunit;

namespace ListLift.ApplicationTests.ListingDrafts;

public sealed class GenerateListingDescriptionHandlerTests
{
    [Fact]
    public async Task HandleAsync_SendsVerifiedFactsAndPriceToGeneratorAndSavesSuggestion()
    {
        var now = DateTimeOffset.UtcNow;
        var draft = ListingDraft.Create("owner-1", "Camera", "Electronics", Money.Create(79.99m, "GBP"), now);
        draft.UpdateProductFacts("Camera", "Electronics", "Used, good condition", "Includes strap", Money.Create(79.99m, "GBP"), now);
        var repository = new FakeListingDraftRepository();
        await repository.AddAsync(draft, CancellationToken.None);
        var generator = new CapturingDescriptionGenerator("A used camera in good condition.");
        var handler = new GenerateListingDescriptionHandler(repository, new FakeCurrentUser("owner-1"), generator, new FakeClock(now));

        var result = await handler.HandleAsync(new GenerateListingDescriptionCommand(draft.Id), CancellationToken.None);

        Assert.Equal("A used camera in good condition.", result);
        Assert.Equal("Camera", generator.LastInput!.ProductName);
        Assert.Equal("Includes strap", generator.LastInput.VerifiedAttributes);
        Assert.Equal(79.99m, generator.LastInput.PriceAmount);
        Assert.Equal("GBP", generator.LastInput.CurrencyCode);
        Assert.Equal(result, draft.Description);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task HandleAsync_OtherOwnersDraftIsNotFoundAndGeneratorIsNotCalled()
    {
        var now = DateTimeOffset.UtcNow;
        var draft = ListingDraft.Create("owner-1", "Camera", "Electronics", Money.Create(79.99m, "GBP"), now);
        var repository = new FakeListingDraftRepository();
        await repository.AddAsync(draft, CancellationToken.None);
        var generator = new CapturingDescriptionGenerator("Should not run.");
        var handler = new GenerateListingDescriptionHandler(repository, new FakeCurrentUser("owner-2"), generator, new FakeClock(now));

        await Assert.ThrowsAsync<ListingDraftNotFoundException>(
            () => handler.HandleAsync(new GenerateListingDescriptionCommand(draft.Id), CancellationToken.None));

        Assert.Null(generator.LastInput);
        Assert.Equal(0, repository.SaveCount);
    }

    private sealed class CapturingDescriptionGenerator(string result) : IProductDescriptionGenerator
    {
        public ProductDescriptionInput? LastInput { get; private set; }

        public Task<string> GenerateAsync(ProductDescriptionInput input, CancellationToken cancellationToken)
        {
            LastInput = input;
            return Task.FromResult(result);
        }
    }
}
