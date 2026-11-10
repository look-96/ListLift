namespace ListLift.Application.Abstractions;

public interface IProductDescriptionGenerator
{
    Task<string> GenerateAsync(ProductDescriptionInput input, CancellationToken cancellationToken);
}
