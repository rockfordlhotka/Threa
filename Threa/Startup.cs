using Csla.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Radzen;
using System;
using Threa.Areas.Identity;
using Threa.Dal.MockDb;
using Threa.Dal.SqlServer;
using Threa.Data;
using Threa.Services;

namespace Threa
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddDbContext<ApplicationDbContext>(options =>
          options.UseSqlServer(
              Configuration.GetConnectionString("AzureThrea")));
      services.AddDefaultIdentity<IdentityUser>()
          .AddEntityFrameworkStores<ApplicationDbContext>();
      services.AddTransient<SqlConnection>((a) =>
      {
        var db = new SqlConnection(Configuration.GetConnectionString("AzureThrea"));
        db.Open();
        return db;
      });

      services.AddRazorPages();
      services.AddServerSideBlazor();

      services.Configure<IdentityOptions>(options =>
      {
        // Password settings.
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 6;
        options.Password.RequiredUniqueChars = 1;

        // Lockout settings.
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // User settings.
        options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        options.User.RequireUniqueEmail = true;
      });

      services.AddSingleton<ChatHub>();
      services.AddSingleton<SessionList>();
      services.AddTransient<ChatService>();
      services.AddScoped<CircuitHandler, CircuitSessionService>();
      services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
      services.AddScoped<DialogService>();
      services.AddScoped<NotificationService>();
      //services.AddMockDb();
      services.AddSqlDb();
      services.AddCsla().WithBlazorServerSupport();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        app.UseExceptionHandler("/Error");
      }

      Csla.ApplicationContext.LocalContext.Add("ContentRootPath", env.ContentRootPath);

      app.UseStaticFiles();

      app.UseRouting();

      app.UseAuthentication();
      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapBlazorHub();
        endpoints.MapFallbackToPage("/_Host");
      });
      app.UseCsla();
    }
  }
}
