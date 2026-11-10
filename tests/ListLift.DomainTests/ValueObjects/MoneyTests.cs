using ListLift.Domain.ValueObjects;

namespace ListLift.DomainTests.ValueObjects;

public sealed class MoneyTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_RejectsNonPositiveAmount(decimal amount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Money.Create(amount, "GBP"));
    }

    [Theory]
    [InlineData("GB")]
    [InlineData("GB1")]
    [InlineData("£££")]
    public void Create_RejectsMalformedCurrency(string currency)
    {
        Assert.Throws<ArgumentException>(() => Money.Create(1m, currency));
    }

    [Fact]
    public void Create_NormalizesCurrencyCode()
    {
        Assert.Equal("GBP", Money.Create(1.25m, " gbp ").CurrencyCode);
    }
}
