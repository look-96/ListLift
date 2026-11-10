namespace ListLift.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
