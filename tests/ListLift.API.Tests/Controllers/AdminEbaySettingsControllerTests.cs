using ListLift.API.Controllers;
using ListLift.API.Models;
using ListLift.API.Security;
using ListLift.APITests.TestDoubles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace ListLift.APITests.Controllers;

public sealed class AdminEbaySettingsControllerTests
{
    [Fact]
    public async Task Get_WithoutAdminCredentialReturnsForbidAndDoesNotReadSettings()
    {
        var store = new FakeEbayApplicationSettingsStore();
        var controller = CreateController(store, "expected-key", suppliedKey: null);

        var result = await controller.Get(CancellationToken.None);

        Assert.IsType<ForbidResult>(result);
        Assert.Equal(0, store.GetCount);
    }

    [Fact]
    public async Task Get_WithAdminCredentialReturnsOnlyConfigurationStatus()
    {
        var store = new FakeEbayApplicationSettingsStore();
        var controller = CreateController(store, "expected-key", suppliedKey: "expected-key");

        var result = Assert.IsType<OkObjectResult>(await controller.Get(CancellationToken.None));

        Assert.Equal(1, store.GetCount);
        var status = result.Value!.GetType().GetProperty("isConfigured")!.GetValue(result.Value);
        Assert.Equal(false, status);
        Assert.DoesNotContain("Secret", result.Value.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Set_WithAdminCredentialStoresTrimmedValuesAndReturnsNoContent()
    {
        var store = new FakeEbayApplicationSettingsStore();
        var controller = CreateController(store, "expected-key", suppliedKey: "expected-key");

        var result = await controller.Set(new SetEbaySettingsRequest
        {
            ClientId = "  production-client-id  ",
            ClientSecret = "  production-client-secret  "
        }, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(1, store.SetCount);
        Assert.Equal("production-client-id", store.Credentials!.ClientId);
        Assert.Equal("production-client-secret", store.Credentials.ClientSecret);
    }

    [Fact]
    public async Task Delete_WithAdminCredentialRemovesStoredValues()
    {
        var store = new FakeEbayApplicationSettingsStore();
        await store.SetAsync(new("client-id", "secret-value"), CancellationToken.None);
        var controller = CreateController(store, "expected-key", suppliedKey: "expected-key");

        var result = await controller.Delete(CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(1, store.DeleteCount);
        Assert.Null(store.Credentials);
    }

    [Fact]
    public void Controller_DeclaresAuthenticationAndAdminRateLimit()
    {
        var controllerType = typeof(AdminEbaySettingsController);

        Assert.NotNull(Attribute.GetCustomAttribute(controllerType, typeof(AuthorizeAttribute)));
        var rateLimit = Assert.IsType<EnableRateLimitingAttribute>(
            Attribute.GetCustomAttribute(controllerType, typeof(EnableRateLimitingAttribute)));
        Assert.Equal("Admin", rateLimit.PolicyName);
    }

    private static AdminEbaySettingsController CreateController(
        FakeEbayApplicationSettingsStore store,
        string expectedCredential,
        string? suppliedKey)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(expectedCredential)));
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Admin:CredentialHash"] = hash })
            .Build();
        var controller = new AdminEbaySettingsController(store, new AdminCredentialVerifier(configuration));
        var context = new DefaultHttpContext();
        if (suppliedKey is not null)
            context.Request.Headers["X-ListLift-Admin-Key"] = suppliedKey;
        controller.ControllerContext = new ControllerContext { HttpContext = context };
        return controller;
    }
}
