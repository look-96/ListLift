namespace ListLift.Application.Abstractions;

public interface IListingPhotoStorage
{
    Task<string> StoreAsync(
        ReadOnlyMemory<byte> content,
        string mediaType,
        CancellationToken cancellationToken);

    Task DeleteAsync(string storageKey, CancellationToken cancellationToken);
}
