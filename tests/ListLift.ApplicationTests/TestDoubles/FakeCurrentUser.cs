using ListLift.Application.Abstractions;

namespace ListLift.ApplicationTests.TestDoubles;

internal sealed class FakeCurrentUser(string userId) : ICurrentUser
{
    public string UserId { get; } = userId;
}
