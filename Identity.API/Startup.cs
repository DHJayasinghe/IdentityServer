using CommonUtil;
using CommonUtil.Helpers;
using Identity.API.Common;
using Identity.API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
               .AddCustomIdentity(Configuration)
               .AddCustomAuthentication(Configuration)
               .AddCustomMvc(Configuration)
               .AddSwaggerApiDocumentation();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity Server API v1");
            });

            // apply database schema migrations & initial data seeding
            PrepareDatabase.PrepPopulation(app);
        }
    }

    static class ServiceCollectionExtensions
    {
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
            var authSettings = configuration.GetSection(nameof(AuthSettings));
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(configureOptions =>
            {
                configureOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "Id", // map custom "Id" claim to "User.Identity.Name", else User.Identity.Name will retrieve null
                    ValidateIssuer = true,
                    ValidIssuer = authSettings[nameof(AuthSettings.Issuer)],
                    ValidateAudience = true,
                    ValidAudience = authSettings[nameof(AuthSettings.Audience)],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                       Encoding.ASCII.GetBytes(authSettings[nameof(AuthSettings.SecretKey)])
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

        public static IServiceCollection AddSwaggerApiDocumentation(this IServiceCollection services)
        {
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Identity Server API",
                    Version = "v1",
                    Description = "An API to perform IAM (Identity Access Management) operations",
                    Contact = new OpenApiContact
                    {
                        Name = "Dhanuka Jayasinghe",
                        Email = "hasitha2kandy@gmail.com"
                    }
                });
                var security = new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer", new string[] { }},
                };
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Scheme = "Bearer",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }

                        },
                        new string[]{}
                    }
                });
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            return services;
        }
    }
}
