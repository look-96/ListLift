namespace ListLift.Application.Abstractions;

public interface IListingPhotoProcessor
{
    Task<ProcessedListingPhoto> ProcessAsync(Stream source, CancellationToken cancellationToken);
}
