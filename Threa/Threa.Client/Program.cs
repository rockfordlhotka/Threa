using Csla.Configuration;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Threa.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore();

builder.Services.AddTransient<RenderModeProvider>();
builder.Services.AddScoped<ActiveCircuitState>();

builder.Services.AddCsla(o => o
  .AddBlazorWebAssembly(o => o.SyncContextWithServer = true)
  .DataPortal(o => o.AddClientSideDataPortal(o => o
    .UseHttpProxy(o => o.DataPortalUrl = "/api/dataportal"))));

await builder.Build().RunAsync();
