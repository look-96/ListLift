using ListLift.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ListLift.InfrastructureTests.GoogleCloud;

public sealed class CloudRunDatabaseRegistrationTests
{
    [Fact]
    public void AddListLiftDatabase_RequiresExplicitPostgreSqlProviderOnCloudRun()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["K_SERVICE"] = "listlift-dev",
            ["ConnectionStrings:ListingDb"] = "Host=localhost;Database=listlift;Username=listlift;Password=local"
        });

        var exception = Assert.Throws<InvalidOperationException>(
            () => new ServiceCollection().AddListLiftDatabase(configuration));

        Assert.Contains("Database:Provider must be PostgreSql", exception.Message);
    }

    [Fact]
    public void AddListLiftDatabase_RejectsSqlServerProviderOnCloudRun()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["K_REVISION"] = "listlift-dev-00001",
            ["Database:Provider"] = "SqlServer",
            ["ConnectionStrings:ListingDb"] = "Server=localhost;Database=listlift"
        });

        var exception = Assert.Throws<InvalidOperationException>(
            () => new ServiceCollection().AddListLiftDatabase(configuration));

        Assert.Contains("Database:Provider must be PostgreSql", exception.Message);
    }

    private static IConfiguration CreateConfiguration(IDictionary<string, string?> settings) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
}
