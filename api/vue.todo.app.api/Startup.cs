using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            var client = this._configuration.GetValue<string>("Client");

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins(client).AllowCredentials();
                    builder.WithOrigins(client).AllowAnyMethod();
                    builder.WithOrigins(client).AllowAnyHeader();
                });
            });
            services.AddControllers();

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
            app.UseEndpoints(config =>
            {
                config.MapHub<ChangesListenerHub>("/tasks");
                config.MapControllers();
            });
        }
    }
}