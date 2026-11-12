using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Globalization;
using ListLift.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
var ukCulture = CultureInfo.GetCultureInfo("en-GB");
CultureInfo.DefaultThreadCurrentCulture = ukCulture;
CultureInfo.DefaultThreadCurrentUICulture = ukCulture;
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"];
var clientBaseUri = new Uri(builder.HostEnvironment.BaseAddress, UriKind.Absolute);
if (string.IsNullOrWhiteSpace(apiBaseUrl)
    || !Uri.TryCreate(clientBaseUri, apiBaseUrl, out var apiUri)
    || apiUri is null)
{
    throw new InvalidOperationException("ApiBaseUrl must resolve to an absolute HTTPS URL.");
}

if (apiUri.Scheme != Uri.UriSchemeHttps)
    throw new InvalidOperationException("ApiBaseUrl must resolve to HTTPS.");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = apiUri });
builder.Services.AddScoped<ListLift.Client.Services.AuthSession>();
builder.Services.AddScoped<ListLift.Client.Services.ListLiftApiClient>();
builder.Services.AddScoped<ListLift.Client.Services.DraftRecoveryStore>();

await builder.Build().RunAsync();
