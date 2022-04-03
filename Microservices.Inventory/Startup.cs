using Microservices.Common.Extensions;
using Microservices.Common.Interfaces;
using Microservices.Common.Types;
using Microservices.Inventory.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microservices.Inventory
{
    public class Startup
    {
        //Use in Configure() method to set CORS
        private const string AllowedOriginSettings = "AllowedOrigin";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMongo()
                .AddMongoRepository<InventoryItem>("UserInventory")
                .AddMongoRepository<CatalogItem>("InventoryItems")
                .AddMassTransitRabbitMQ();

            services.AddCustomHttpClient(new HttpClientParameters { baseUrl= "https://localhost:44317",timeoutPolicy=2 });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Microservices.Inventory", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Microservices.Inventory v1"));

                //Add Cross Origin Requests to allow UI from different hosted web server calls.
                app.UseCors(configuration =>
                {
                    //retrieve from app.Development.Settings.json with AllowedOriginSettings const key from line 26 the allowed origins urls
                    //allow and header (content-type etc.) and method (post,get etc.)
                    configuration.WithOrigins(Configuration[AllowedOriginSettings])
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
