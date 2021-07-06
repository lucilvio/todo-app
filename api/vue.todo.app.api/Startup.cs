using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Vue.TodoApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services) {}

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCustomExceptionHandler();
            app.UseRouting();
            app.UseEndpoints(TasksApi.Map);
        }
    }
}