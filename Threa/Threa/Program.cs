using Csla.Configuration;
using GameMechanics;
using GameMechanics.Messaging;
using GameMechanics.Messaging.InMemory;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Server.Circuits;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Radzen;
using Threa.Components;
using Threa.Dal;
using Threa.Services;
using Threa.Client.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
  .AddCookie();
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddTransient<RenderModeProvider>();
builder.Services.AddScoped<ActiveCircuitState>();
builder.Services.AddScoped<CircuitIdProvider>();
builder.Services.AddSingleton<PlayerConnectionTracker>();
builder.Services.AddScoped(typeof(CircuitHandler), typeof(ActiveCircuitHandler));

builder.Services.AddCsla(o => o
    .AddAspNetCore()
    .AddServerSideBlazor(o => o.UseInMemoryApplicationContextManager = true));

builder.Services.AddSqlite();
builder.Services.AddGameMechanics();
builder.Services.AddInMemoryMessaging();

// Targeting system
builder.Services.AddSingleton<TargetingInteractionManager>();
builder.Services.AddSingleton<ITargetingInteractionManager>(sp => sp.GetRequiredService<TargetingInteractionManager>());

// Radzen services
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();

// NPC auto-naming service (session-scoped)
builder.Services.AddScoped<NpcAutoNamingService>();

// Health checks for Kubernetes probes
builder.Services.AddHealthChecks();

// OpenTelemetry — exports traces, metrics, and logs to Grafana Alloy via OTLP
var otelConfig = builder.Configuration.GetSection("OpenTelemetry");
var serviceName = otelConfig["ServiceName"] ?? "threa";
var otlpEndpoint = otelConfig["OtlpEndpoint"] ?? "http://localhost:4317";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(serviceName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint)))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint)));

builder.Logging.AddOpenTelemetry(o =>
{
    o.IncludeScopes = true;
    o.AddOtlpExporter(exporter => exporter.Endpoint = new Uri(otlpEndpoint));
});

var app = builder.Build();

// Reset all connection statuses on startup since no circuits exist yet
using (var scope = app.Services.CreateScope())
{
    var tableDal = scope.ServiceProvider.GetRequiredService<ITableDal>();
    await tableDal.ResetAllConnectionStatusesAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.MapStaticAssets();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Threa.Client.Components._Imports).Assembly);

app.Run();
