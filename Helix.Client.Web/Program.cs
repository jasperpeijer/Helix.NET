using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Helix.Client.Web;
using Helix.Client.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<Helix.Client.Web.Services.AuthService>();
builder.Services.AddScoped<ApiAuthenticationStateProvider>();

builder.Services.AddTransient<Helix.Client.Web.Services.AuthorizationMessageHandler>();
builder.Services.AddHttpClient("HelixAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7096");
})
.AddHttpMessageHandler<Helix.Client.Web.Services.AuthorizationMessageHandler>();
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("HelixAPI"));

builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();