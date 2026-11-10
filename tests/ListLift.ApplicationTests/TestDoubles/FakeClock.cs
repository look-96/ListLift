using ListLift.Application.Abstractions;

namespace ListLift.ApplicationTests.TestDoubles;

internal sealed class FakeClock(DateTimeOffset utcNow) : IClock
{
    public DateTimeOffset UtcNow { get; set; } = utcNow;
}
