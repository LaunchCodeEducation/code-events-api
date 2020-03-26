using CodeEventsAPI.Data;
using CodeEventsAPI.Middleware;
using CodeEventsAPI.Models;
using CodeEventsAPI.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace CodeEventsAPI {
  public class Startup {
    public Startup(IConfiguration configuration) {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services) {
      // TODO: add to keyvault on deploy
      // used to generate absolute resource links
      ServerConfig.Origin = Configuration.GetValue<string>("ServerOrigin");

      // configure routing
      services.AddMvc();
      services.AddControllers();

      // add coordinator services
      services.AddScoped<CodeEventService>();

      // configure DB
      services.AddDbContext<CodeEventsDbContext>(
        dbOptions => dbOptions.UseMySql(Configuration.GetConnectionString("Default"))
      );

      // configure ADB2C auth handling
      services.AddAuthentication(AzureADB2CDefaults.AuthenticationScheme)
        .AddAzureADB2C(options => Configuration.Bind("AzureAdB2C", options));
      services.AddOptions();
      services.Configure<AzureADB2COptions>(Configuration.GetSection("AzureAdB2C"));

      // configure swagger documentation
      services.AddSwaggerGen(
        options => {
          // generate the swagger doc
          options.SwaggerDoc(
            "v1",
            new OpenApiInfo {
              Version = "v1",
              Title = "Code Events API",
              Description = "REST API for managing Code Events"
            }
          );
          // req/res annotations
          options.EnableAnnotations();
          // req/res body examples
          options.ExampleFilters();
          options.GeneratePolymorphicSchemas();
        }
      );

      // register swagger example response objects
      services.AddSwaggerExamplesFromAssemblyOf<NewCodeEventExample>();
      services.AddSwaggerExamplesFromAssemblyOf<PublicCodeEventExample>();
      services.AddSwaggerExamplesFromAssemblyOf<PublicCodeEventExamples>();
      services.AddSwaggerExamplesFromAssemblyOf<MemberCodeEventExample>();
      services.AddSwaggerExamplesFromAssemblyOf<MemberCodeEventExamples>();
      services.AddSwaggerExamplesFromAssemblyOf<MemberExample>();
      services.AddSwaggerExamplesFromAssemblyOf<MemberExamples>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
      if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

      // routing (middleware: ORDER MATTERS!)
      app.UseRouting(); // use the routing middleware
      app.UseAuthentication(); // authenticate first
      app.UseAuthorization(); // authorize next

      app.UseMiddleware<RegisterNewUserMiddleware>();
      app.UseMiddleware<AddUserIdClaimMiddleware>();

      app.UseEndpoints(endpoints => endpoints.MapControllers()); // continue to Controller handlers

      // configure swagger UI page
      app.UseSwagger();
      app.UseSwaggerUI(
        options => options.SwaggerEndpoint(
          "/swagger/v1/swagger.json",
          "Code Events API Documentation"
        )
      );


      // automatic DB migrations
      using var migrationSvcScope = app.ApplicationServices
        .GetRequiredService<IServiceScopeFactory>()
        .CreateScope();
      migrationSvcScope.ServiceProvider.GetService<CodeEventsDbContext>().Database.Migrate();
    }
  }
}
