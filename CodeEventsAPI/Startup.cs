using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CodeEventsAPI.Data;
using CodeEventsAPI.Middleware;
using CodeEventsAPI.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
using Microsoft.AspNetCore.CookiePolicy;

namespace CodeEventsAPI {
  public class Startup {
    public Startup(IConfiguration configuration) {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services) {
      // configure routing
      services.AddMvc();
      services.AddControllers();

      // configure DB
      services.AddDbContext<CodeEventsDbContext>(dbOptions =>
        dbOptions.UseMySql(Configuration.GetConnectionString("Default")));

      services.AddAuthentication(AzureADB2CDefaults.AuthenticationScheme)
        .AddAzureADB2C(options => Configuration.Bind("AzureAdB2C", options));
      services.AddOptions();
      services.Configure<AzureADB2COptions>(
        Configuration.GetSection("AzureAdB2C"));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
      if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

      // routing (middleware: ORDER MATTERS!)
      app.UseRouting(); // use the routing middleware
      app.UseAuthentication(); // authenticate first
      app.UseAuthorization(); // authorize next

      app.UseMiddleware<RegisterNewUserMiddleware>();
      app.UseMiddleware<AddUserIdClaimMiddleware>();

      app.UseEndpoints(endpoints =>
        endpoints.MapControllers()); // continue to Controller handlers


      // automatic DB migrations
      using var migrationSvcScope = app.ApplicationServices
        .GetRequiredService<IServiceScopeFactory>()
        .CreateScope();
      migrationSvcScope.ServiceProvider.GetService<CodeEventsDbContext>()
        .Database.Migrate();
    }
  }
}