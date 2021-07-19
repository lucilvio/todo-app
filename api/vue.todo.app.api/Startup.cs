using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

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
            
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins(appSettings.AllowedOrigin).AllowCredentials();                    
                    builder.WithOrigins(appSettings.AllowedOrigin).AllowAnyMethod();
                    builder.WithOrigins(appSettings.AllowedOrigin).AllowAnyHeader();
                    builder.WithOrigins(appSettings.AllowedOrigin).SetPreflightMaxAge(TimeSpan.FromHours(12));
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

            services.AddSignalR();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors();

            app.UseRouting();
            app.UseAuthentication();

            app.UseEndpoints(config =>
            {
                config.MapHub<ChangesListenerHub>("/change-listener");
                config.MapControllers();
            });
        }
    }
}