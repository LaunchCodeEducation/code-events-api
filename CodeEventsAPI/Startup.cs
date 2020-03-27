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
      // services.AddAuthentication(AzureADB2CDefaults.AuthenticationScheme)
      //   .AddAzureADB2C(options => Configuration.Bind("AzureAdB2C", options));
      // services.Configure<AzureADB2COptions>(Configuration.GetSection("AzureAdB2C"));
      // services.AddOptions();

      services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(
          jwtOptions => {
            jwtOptions.Authority =
              // $"https://login.microsoftonline.com/{Configuration["AzureAdB2C:TenantId"]}/{Configuration["AzureAdB2C:Policy"]}/v2.0/";
              $"https://login.microsoftonline.com/{Configuration["AzureAdB2C:TenantId"]}/v2.0/";
            jwtOptions.Audience = Configuration["AzureAdB2C:ClientId"];
          }
        );

      // configure swagger documentation
      services.AddSwaggerGen(
        options => {
          // generate the swagger doc
          // TODO: have document attached bearer header
          options.AddSecurityRequirement(
            new OpenApiSecurityRequirement {
              {
                new OpenApiSecurityScheme {
                  Reference = new OpenApiReference {
                    Id = "oauth2",
                    Type = ReferenceType.SecurityScheme,
                  },
                  UnresolvedReference = true,
                  // Name = "Bearer",
                  // Scheme = JwtBearerDefaults.AuthenticationScheme,
                },
                new List<string>()
              }
            }
          );
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
          // TODO: requires clicking this link to complete and get cookie
          // https://patrickcodeevents.b2clogin.com/patrickcodeevents.onmicrosoft.com/b2c_1_code_events_signup_signin/oauth2/v2.0/authorize?client_id=06eb34fd-455b-4084-92c3-07d5389e6c15&redirect_uri=https%3A%2F%2Flocalhost%3A5001%2Fauth%2Fsuccess&response_type=id_token&scope=openid%20profile&response_mode=form_post&nonce=637209388333821120.ZGQyZDUwZmYtNzY3Yy00ZjJkLTg3YWMtZWYwMTBhYWJiMmUyZjhiMzRmMGItMzExNy00N2Q5LTgzYzQtNmY1Mzk5NTIxZjdi&state=CfDJ8MSw_mFNEoJHi0RQnwx9-eNEcXKrQwjBzAGDX-03ujE0noK9IjlpRFFc1Kzrl46GlMI0a9V64Ogrhy8uFi5P2Y6pwhFfFefQarQCd4HwBn1w_Eu69WwsJ5OqPAucCUDj3UVvOoeSDQOx0d0yW9Rfmf2D9Kn9mSjNQNOwu5mwqJPDTd8KI8KOHlmKRHhYf3wAkhnH4mgHox9wvK1F5NgQhfV3jTyBXDI9AX78JdrgnAi-LQE7MVE5eOnhfn7sdyf18dgUbMRm4bIRq1nR2ULKj7EwBm5fowMQbjRdX7EtqcQ3gOlkJs2eeUMv5bhZxjUZ4Q&x-client-SKU=ID_NETSTANDARD2_0&x-client-ver=5.5.0.0
          options.AddSecurityDefinition(
            "oauth2",
            new OpenApiSecurityScheme {
              Type = SecuritySchemeType.OAuth2,
              Flows = new OpenApiOAuthFlows {
                Implicit = new OpenApiOAuthFlow {
                  //AuthorizationUrl = new System.Uri("https://patrickcodeevents.b2clogin.com/patrickcodeevents.onmicrosoft.com/b2c_1_code_events_signup_signin/oauth2/v2.0/authorize?client_id=06eb34fd-455b-4084-92c3-07d5389e6c15&redirect_uri=https%3A%2F%2Flocalhost%3A5001%2Fauth%2Fsuccess&response_type=id_token&scope=openid%20profile&response_mode=form_post&nonce=637208568163422217.OTAwNGQ1ZDEtMDAzOC00ZWM0LTllYTktZmVkMjFlZWIzNmE5ZWQzZjAzZDctYWYxMC00MmJlLTkxMDUtMzg5Y2M2ZWVjN2Ri&state=CfDJ8JkAwUPuIRhNraWx2fBSKAIjzH_3f-HGf4GIs7ca5c4GgwFrp-Kf_AnmL1rqTjc7ZrPqFlx10ll7wKFBhdjRdsycTBMOBP1EazR_bMIawDzFqT7onmiGBw4-bNnPyUSYqqc8awd_nFuGsuKqRsp3SLRg_yC5sG-x8YvAGpDPN_g-xhhWxp7JlNxVm0lZuIOz3wy1D1E-DA2NswLE1Fu2J3PJaiChKGlx2_OoHdnFZKgzLhox-Ibfob-XfDI7x2GJfWvG_FmB4b9_sMKGxwGn7qY7VjWoeAADMiI3cRWqZjOfF65pudqE3rkkb3EeCroxCw&x-client-SKU=ID_NETSTANDARD2_0&x-client-ver=5.5.0.0", System.UriKind.Absolute),
                  //AuthorizationUrl = new System.Uri("/AzureADB2C/Account/SignIn/AzureADB2C", System.UriKind.Relative),
                  AuthorizationUrl = new System.Uri(
                    "https://patrickcodeevents.b2clogin.com/patrickcodeevents.onmicrosoft.com/oauth2/v2.0/authorize?p=B2C_1_code_events_signup_signin",
                    // $"https://login.microsoftonline.com/{Configuration["AzureAdB2C:TenantId"]}/oauth2/v2.0/authorize",
                    System.UriKind.Absolute
                  ),
                  // TokenUrl = new System.Uri(
                  //   "https://patrickcodeevents.b2clogin.com/patrickcodeevents.onmicrosoft.com/oauth2/v2.0/token?p=B2C_1_code_events_signup_signin",
                  //   System.UriKind.Absolute
                  // ),
                  Scopes = new Dictionary<string, string> {
                    {
                      "https://patrickcodeevents.onmicrosoft.com/code-events/user_impersonation",
                      "Access Swagger on behalf of the CodeEvents signed in user"
                    }, {
                      "https://patrickcodeevents.onmicrosoft.com/code-events/api_access",
                      "Access the CodeEventsAPI"
                    },
                  }
                }
              }
            }
          );
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
        options => {
          options.SwaggerEndpoint("/swagger/v1/swagger.json", "Code Events API Documentation");
          //   // TODO: add to secrets
          // options.OAuthClientId("e6246ac7-4359-4a54-b2a5-7b94aeeb8de6");
          options.OAuthClientId("06eb34fd-455b-4084-92c3-07d5389e6c15");
          options.OAuthAppName("code-events-swagger-ui");
          // options.OAuthConfigObject = new OAuthConfigObject {
          //   ClientId = "e6246ac7-4359-4a54-b2a5-7b94aeeb8de6",
          //   AppName = "code-events-swagger-ui"
          // };
          // options.OAuthUseBasicAuthenticationWithAccessCodeGrant();
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
