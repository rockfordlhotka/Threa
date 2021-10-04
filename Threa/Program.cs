using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Threa.Data;
using Csla.Configuration;
using Threa.Areas.Identity;
using Threa.Dal.MockDb;
using Threa.Dal.SqlServer;
using Threa.Services;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;

var builder = WebApplication.CreateBuilder(args);
IConfiguration Configuration = null;
IWebHostEnvironment env = null;

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        Configuration.GetConnectionString("AzureThrea")));
builder.Services.AddDefaultIdentity<IdentityUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddTransient<SqlConnection>((a) =>
{
  var db = new SqlConnection(Configuration.GetConnectionString("AzureThrea"));
  db.Open();
  return db;
});

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddSingleton<ChatHub>();
builder.Services.AddSingleton<SessionList>();
builder.Services.AddTransient<ChatService>();
builder.Services.AddScoped<CircuitHandler, CircuitSessionService>();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
//services.AddMockDb();
builder.Services.AddSqlDb();
builder.Services.AddCsla().WithBlazorServerSupport();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Error");
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

Csla.ApplicationContext.LocalContext.Add("ContentRootPath", env.ContentRootPath);

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();


app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.UseCsla();

app.Run();
