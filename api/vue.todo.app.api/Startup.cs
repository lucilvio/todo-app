using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Vue.TodoApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services) 
        {
            services.AddCors();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCustomExceptionHandler();
            
            app.UseCors(config => {                
                config.AllowAnyHeader();
                config.AllowAnyMethod();
                config.AllowAnyOrigin();
            });
            
            app.UseRouting();            
            app.UseEndpoints(TasksApi.Map);
        }
    }
}