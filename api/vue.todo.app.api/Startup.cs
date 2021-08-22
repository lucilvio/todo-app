using System;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;

namespace Vue.TodoApp
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var appSettings = new AppSettings();
            var appSettingsSection = this._configuration.GetSection("AppSettings");

            appSettingsSection.Bind(appSettings);
            services.Configure<AppSettings>(appSettingsSection);

            services.AddApplicationInsightsTelemetry();
            
            services.AddLogging(config =>
            {
                config.ClearProviders();
                config.AddApplicationInsights();
            });


            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins(appSettings.AllowedOrigin).AllowCredentials();
                    builder.WithOrigins(appSettings.AllowedOrigin).AllowAnyMethod();
                    builder.WithOrigins(appSettings.AllowedOrigin).AllowAnyHeader();
                    builder.WithOrigins(appSettings.AllowedOrigin).SetPreflightMaxAge(TimeSpan.FromHours(24));
                });
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidIssuer = appSettings.Jwt.Issuer,
                        ValidAudience = appSettings.Jwt.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(appSettings.Jwt.Secret))
                    };
                });

            services.AddAuthorization();

            services.AddHealthChecks()
                .AddCheck<HealthChecker>("health");

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSingleton<JwtTokenGenerator>();
            services.AddSingleton<Auth>();

            services.AddControllers(options =>
            {
                options.Filters.Add(new AuthorizeFilter());
            });

            services.AddDbContext<TodoAppContext>(options =>
            {
                options.UseSqlite("Data Source=todoapp.db");
            });

            services.AddHttpClient();

            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors();
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (BusinessException ex)
                {
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await context.Response.WriteAsJsonAsync(new { ErrorMessage = ex.Message });
                }
            });

            app.UseEndpoints(config =>
            {
                config.MapControllers();
                config.MapHub<NotifyHub>("/notify");
                config.MapHealthChecks("/health", new HealthCheckOptions
                {
                    AllowCachingResponses = false
                });
            });
        }
    }
}