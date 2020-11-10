using CommonUtil;
using CommonUtil.Helpers;
using HealthChecks.UI.Client;
using Identity.API.Common;
using Identity.API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace Identity.API
{
    public sealed class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString = Configuration["ASPNETCORE_ENVIRONMENT"] == "Production"
              ? string.Format("Server={0},{1};Initial Catalog={2};User ID={3};Password={4};",
                  Configuration["DB_SERVER"], Configuration["DB_PORT"], Configuration["DATABASE"],
                  Configuration["DB_USER"], Configuration["DB_PASSWORD"])
              : Configuration.GetConnectionString("IdentityDB"); // for development environment log-in using windows authentication (without credentials)
            
            services
               .AddCustomDbContext(Configuration, connectionString)
               .AddCustomConfiguration(Configuration)
               .AddHealthChecks(Configuration, connectionString)
               .AddCustomIdentity(Configuration)
               .AddCustomAuthentication(Configuration)
               .AddCustomMvc(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // HealthCheck middleware, put before Serilog middleware to avoid unnecessary health check verbose logs
            app.UseHealthChecks("/hc", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());

            PrepareDatabase.PrepPopulation(app);
        }
    }

    static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration, string connectionString)
        {
            var hcBuilder = services.AddHealthChecks();

            hcBuilder.AddCheck("self", () => HealthCheckResult.Healthy());

            hcBuilder.AddSqlServer(
                    connectionString,
                    name: "identitydb-check",
                    tags: new string[] { "identitydb", "connectionstring" });

            return services;
        }

        public static IServiceCollection AddCustomDbContext(this IServiceCollection services, IConfiguration configuration, string connectionString)
        {
            services
               .AddDbContext<IdentityDBContext>(options =>
                   options.UseSqlServer(connectionString,
                       sqlServerOptionsAction: sqlOptions =>
                       {
                           // configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                           sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                       }),
                       ServiceLifetime.Scoped //Showing explicitly that the DbContext is shared across the HTTP request scope (graph of objects started in the HTTP request)
                )
               .AddScoped(typeof(UnitOfWork), typeof(UnitOfWork));

            return services;
        }

        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(configureOptions =>
            {
                configureOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "Id", // map custom "Id" claim to "User.Identity.Name", else User.Identity.Name will retrieve null
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.ASCII.GetBytes(configuration.GetSection(nameof(AuthSettings))[nameof(AuthSettings.SecretKey)])
                    ), //signin key
                    RequireExpirationTime = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                configureOptions.SaveToken = true;
            });

            return services;
        }

        public static IServiceCollection AddCustomConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            //register delegating handlers
            services.AddHttpContextAccessor();

            // configure strongly typed settings objects
            services.AddScoped(typeof(ICurrentUser), typeof(CurrentUser));
            services.Configure<AuthSettings>(configuration.GetSection(nameof(AuthSettings)));

            return services;
        }

        public static IServiceCollection AddCustomMvc(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration["ASPNETCORE_ENVIRONMENT"] != "Production")  // on non-production, run app using kestrel configurations (host & port) on appsettings.json
            {
                services.Configure<KestrelServerOptions>(configuration.GetSection("Kestrel"));
            }

            services.AddControllers();

            return services;
        }

        public static IServiceCollection AddCustomIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration.GetSection(nameof(AuthSettings))[nameof(AuthSettings.SecretKey)]));
            // jwt wire up. Get options from app settings
            var jwtAppSettingOptions = configuration.GetSection(nameof(JwtIssuerOptions));

            // Configure JwtIssuerOptions
            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                options.Audience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)];
                options.SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            });

            services.AddSingleton(typeof(IJwtFactory), typeof(JwtFactory));
            services.AddSingleton(typeof(IJwtTokenHandler), typeof(JwtTokenHandler));
            services.AddSingleton(typeof(ITokenFactory), typeof(TokenFactory));
            services.AddSingleton(typeof(IJwtTokenValidator), typeof(JwtTokenValidator));

            return services;
        }
    }
}
