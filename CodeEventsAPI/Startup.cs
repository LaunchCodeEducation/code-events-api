using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CodeEventsAPI.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pomelo.EntityFrameworkCore.MySql;

namespace CodeEventsAPI {
  public class Startup {
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration) {
      Configuration = configuration;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services) {
      services.AddMvc();
      services.AddControllers();
      services.AddDbContext<CodeEventsDbContext>(dbOptions =>
        dbOptions.UseMySql(Configuration.GetConnectionString("Default")));
    }

    // TODO: auto run migrations?

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
      if (env.IsDevelopment()) {
        app.UseDeveloperExceptionPage();
      }

      app.UseRouting();
      app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
  }
}