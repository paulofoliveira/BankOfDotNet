using BankOfDotNet.API.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;

namespace BankOfDotNet.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Configurando Autenticação para o IdentityServer configurado.

            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "http://localhost:5000";
                    options.RequireHttpsMetadata = false;
                    options.ApiName = "bankOfDotNetApi";
                });

            services.AddDbContext<BankContext>(opts => opts.UseInMemoryDatabase("BankingDb"));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // Swagger:

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info()
                {
                    Title = "BankOfDotNet API",
                    Version = "v1"
                });

                options.OperationFilter<CheckAuthorizeOperationFilter>();

                options.AddSecurityDefinition("oauth2", new OAuth2Scheme()
                {
                    Type = "oauth2",
                    Flow = "implicit",
                    AuthorizationUrl = "http://localhost:5000/connect/authorize",
                    TokenUrl = "http://localhost:5000/connect/token",
                    Scopes = new Dictionary<string, string>()
                    {
                        { "bankOfDotNetApi", "Customer API for BankOfDotNet" }
                    }
                });
            });
        }


        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();

            app.UseMvc();

            app.UseSwaggerUI(options=>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "BankOfDotNet API V1");
                options.OAuthClientId("swaggerclientui");
                options.OAuthAppName("Swagger Client API");
            });
        }
    }
}
