using System.Collections.Generic;
using CodeEventsAPI.Data;
using CodeEventsAPI.Middleware;
using CodeEventsAPI.Services;
using CodeEventsAPI.Swagger;
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
using Swashbuckle.AspNetCore.SwaggerUI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Collections;

namespace CodeEventsAPI {
  public class Startup {
    public Startup(IConfiguration configuration) {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services) {
      // used to generate absolute URL resource links
      ServerConfig.Origin = Configuration.GetValue<string>("ServerOrigin");

      // configure routing
      services.AddMvc();
      services.AddControllers();

      // add coordinator services
      services.AddScoped<IMemberService, MemberService>();
      services.AddScoped<ICodeEventService, CodeEventService>();
      services.AddScoped<IUserTransferenceService, UserTransferenceService>();

      // configure DB
      services.AddDbContext<CodeEventsDbContext>(
        dbOptions => dbOptions.UseMySql(Configuration.GetConnectionString("Default"))
      );

      services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(
          jwtOptions => {
            jwtOptions.Audience = Configuration["AzureAdB2C:ClientId"];
            jwtOptions.Authority =
              $"{Configuration["AzureAdB2C:Instance"]}/{Configuration["AzureAdB2C:TenantId"]}/{Configuration["AzureAdB2C:Policy"]}/v2.0/";
          }
        );

      // configure swagger documentation
      services.AddSwaggerGen(
        options => {
          options.SwaggerDoc(
            "v1",
            new OpenApiInfo {
              Version = "v1",
              Title = "Code Events API",
              Description = "REST API for managing Code Events"
            }
          );

          // source of truth for reference used in SecurityRequirement and SecurityDefinition
          const string securityId = "adb2c";

          // instructs swagger to add token as Authorization header (Bearer <token>)
          options.AddSecurityRequirement(
            new OpenApiSecurityRequirement {
              {
                new OpenApiSecurityScheme {
                  Reference = new OpenApiReference {
                    Id = securityId,
                    Type = ReferenceType.SecurityScheme,
                  },
                  UnresolvedReference = true,
                },
                new List<string>()
              }
            }
          );

          // define the oauth flow for swagger to use
          options.AddSecurityDefinition(
            securityId,
            new OpenApiSecurityScheme {
              Type = SecuritySchemeType.OAuth2,
              Flows = new OpenApiOAuthFlows {
                Implicit = new OpenApiOAuthFlow {
                  AuthorizationUrl = new System.Uri(
                    Configuration["AzureAdB2C:AuthorizationUrl"],
                    System.UriKind.Absolute
                  ),
                  Scopes = new Dictionary<string, string> {
                    {
                      $"{Configuration["AzureAdB2C:Domain"]}/{Configuration["AzureAdB2C:AppName"]}/user_impersonation",
                      "Access Swagger on behalf of the CodeEvents signed in user"
                    }, {
                      $"{Configuration["AzureAdB2C:Domain"]}/{Configuration["AzureAdB2C:AppName"]}/api_access",
                      "Access the CodeEventsAPI"
                    }
                  }
                }
              }
            }
          );

          // req/res annotations
          options.EnableAnnotations();

          // req/res body examples
          options.ExampleFilters();
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

      app.UseMiddleware<AddUserIdClaimMiddleware>();

      app.UseEndpoints(endpoints => endpoints.MapControllers()); // continue to Controller handlers

      // configure swagger UI page
      app.UseSwagger();
      app.UseSwaggerUI(
        options => {
          options.SwaggerEndpoint("/swagger/v1/swagger.json", "Code Events API Documentation");
          options.OAuthClientId(Configuration["AzureAdB2C:ClientId"]);
        }
      );


      // automatic DB migrations
      using var migrationSvcScope = app.ApplicationServices
        .GetRequiredService<IServiceScopeFactory>()
        .CreateScope();
      migrationSvcScope.ServiceProvider.GetService<CodeEventsDbContext>().Database.Migrate();
    }
  }
}
