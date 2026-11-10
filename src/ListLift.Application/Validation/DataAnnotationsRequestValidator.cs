using System.ComponentModel.DataAnnotations;

namespace ListLift.Application.Validation;

public static class DataAnnotationsRequestValidator
{
    public static void Validate<TRequest>(TRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Validator.ValidateObject(
            request,
            new ValidationContext(request),
            validateAllProperties: true);
    }
}
