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
if (!Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var apiUri) || apiUri.Scheme != Uri.UriSchemeHttps)
    throw new InvalidOperationException("ApiBaseUrl must be an absolute HTTPS URL.");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = apiUri });
builder.Services.AddScoped<ListLift.Client.Services.AuthSession>();
builder.Services.AddScoped<ListLift.Client.Services.ListLiftApiClient>();
builder.Services.AddScoped<ListLift.Client.Services.DraftRecoveryStore>();

await builder.Build().RunAsync();
