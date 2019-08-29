using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Linq;
using System.Reflection;

// Using ASP.NET Core Identity
// https://identityserver4test.readthedocs.io/en/latest/quickstarts/6_aspnet_identity.html

// Identity Server 4 with ASP.Net Core 2.2
// https://ngohungphuc.wordpress.com/2018/12/11/identity-server-4-with-asp-net-core-2-2/

// Multiple DbContexts, wrong DbContextOptions being injected
// https://github.com/aspnet/EntityFrameworkCore/issues/5132

namespace BankOfDotNet.IdentityServer
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .Build();

            string connectionString = config.GetSection("connectionString").Value;
            var migrationAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            // ASP.NET Identity:

            services.AddDbContext<UserDbContext>(options =>
                 options.UseSqlServer(connectionString));

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<UserDbContext>()
                .AddDefaultTokenProviders();

            //services.AddTransient<IEmailSender, AuthMessageSender>();
            //services.AddTransient<ISmsSender, AuthMessageSender>();

            // Configurando IdentityServer:

            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                //.AddInMemoryApiResources(Config.GetAllApiResources())              
                //.AddInMemoryClients(Config.GetClients())
                //.AddInMemoryIdentityResources(Config.GetIdentityResources())
                //.AddTestUsers(Config.GetUsers())
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = o =>
                    o.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationAssembly));
                })
                .AddOperationalStore(options =>
                {
                    // Armazena tokens, consents, codes, etc.

                    options.ConfigureDbContext = o =>
                  o.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationAssembly));
                })

                // Exemplo de ConfigurationStore para tabelas operacionais do IdentityServer4.

                //.AddConfigurationStore<CustomConfigurationDbContext>(opt =>
                //{
                //    opt.ConfigureDbContext = o =>
                //  o.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationAssembly));
                //})
                .AddAspNetIdentity<IdentityUser>();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            InitializeIdentityServerDatabase(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIdentityServer();

            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }

        private void InitializeIdentityServerDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                // Migrate:

                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var configurationDbContext = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                configurationDbContext.Database.Migrate();

                serviceScope.ServiceProvider.GetRequiredService<UserDbContext>().Database.Migrate();

                // Seed:

                // Clients:

                if (!configurationDbContext.Clients.Any())
                {
                    foreach (var client in Config.GetClients())
                    {
                        configurationDbContext.Clients.Add(client.ToEntity());
                    }
                }

                // Api Resources:

                if (!configurationDbContext.ApiResources.Any())
                {
                    foreach (var resource in Config.GetAllApiResources())
                    {
                        configurationDbContext.ApiResources.Add(resource.ToEntity());
                    }
                }

                // Identity Resources:

                if (!configurationDbContext.IdentityResources.Any())
                {
                    foreach (var identityResource in Config.GetIdentityResources())
                    {
                        configurationDbContext.IdentityResources.Add(identityResource.ToEntity());
                    }
                }

                // Users

                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                if (!userManager.Users.Any())
                {
                    foreach (var user in Config.GetUsers())
                    {
                        var identityUser = new IdentityUser(user.Username)
                        {
                            Id = user.SubjectId,
                            EmailConfirmed = true
                        };

                        userManager.CreateAsync(identityUser, "Password123!").Wait();

                        if (user.Claims.Any())
                            userManager.AddClaimsAsync(identityUser, user.Claims.ToList()).Wait();
                    }
                }

                configurationDbContext.SaveChanges();
            }
        }
    }
}
