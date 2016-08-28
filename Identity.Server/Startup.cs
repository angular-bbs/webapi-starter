using Identity.Server.Configurations;
using Identity.Server.Data;
using Identity.Server.Models;
using IdentityModel;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Identity.Server
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true);

            if (env.IsDevelopment())
                builder.AddUserSecrets();

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            Environment = env;
        }

        public IConfigurationRoot Configuration { get; }
        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseIdentity();
            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                Authority = "http://localhost:52338",
                Audience = "http://localhost:52338/",
                RequireHttpsMetadata = false
            });
            app.UseMvc();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Add database
            services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));
            // Identity Server 4
            var identityServerBuilder = services.AddIdentityServer(options => { options.AuthenticationOptions.AuthenticationScheme = "Cookies"; })
                .AddInMemoryClients(Clients.Get())
                .AddInMemoryScopes(Scopes.Get())
                .SetTemporarySigningCredential();

            identityServerBuilder.Services.AddTransient<IProfileService, ProfileService>();
            identityServerBuilder.Services.AddTransient<IResourceOwnerPasswordValidator, ResourceOwnerPasswordValidator>
                ();

            // Add Cors support.
            services.AddCors(
                options =>
                    options.AddPolicy("AllowCorsOrigin",
                        builder =>
                            builder.WithOrigins(ClientConsts.BaseAddress)
                                .AllowAnyMethod()
                                .AllowAnyHeader()
                                .AllowCredentials()));
            // Add Identity
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.Cookies.ApplicationCookie.AuthenticationScheme = "Cookies";
                    options.ClaimsIdentity.UserIdClaimType = JwtClaimTypes.Subject;
                    options.ClaimsIdentity.UserNameClaimType = JwtClaimTypes.Name;
                    options.ClaimsIdentity.RoleClaimType = JwtClaimTypes.Role;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc();

            // Dependency Injections.
            services
                .AddTransient<IUserClaimsPrincipalFactory<ApplicationUser>, IdentityServerUserClaimsPrincipalFactory>();
        }
    }
}