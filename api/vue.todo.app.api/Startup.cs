using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace vue.todo.app.api
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                const string FileName = "tasks.json";

                endpoints.MapGet("/tasks", async context =>
                {
                    using var file = File.Open(FileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
                    
                    IList<Task> tasks = new List<Task>();

                    if(file.Length > 0)
                        tasks = JsonSerializer.Deserialize<IList<Task>>(await File.ReadAllTextAsync(FileName));

                    await context.Response.WriteAsJsonAsync(tasks);
                });

                endpoints.MapPost("/tasks", async context => 
                {
                    using var file = File.Open(FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                    using var sr = new StreamReader(file);

                    IList<Task> tasks = new List<Task>();
                    
                    if(file.Length > 0)
                        tasks = JsonSerializer.Deserialize<IList<Task>>(await sr.ReadToEndAsync());
                    
                    tasks.Add(new Task 
                    {
                        Name = "Sample task"
                    });
                    
                    using var sw = new StreamWriter(file);
                    file.Position = 0;
                    await sw.WriteAsync(JsonSerializer.Serialize(tasks));
                });

                endpoints.MapDelete("/tasks/{id}", async context => 
                {
                    using var file = File.Open(FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                    using var sr = new StreamReader(file);

                    IList<Task> tasks = new List<Task>();
                    var f = context.GetRouteData();

                    Console.WriteLine(f.Values["id"]);
                    if(file.Length > 0)
                        tasks = JsonSerializer.Deserialize<IList<Task>>(await sr.ReadToEndAsync());
                    
                    tasks = tasks.Where(t => t.Id != Guid.Parse(f.Values["id"].ToString())).ToList();
                    
                    using var sw = new StreamWriter(file);
                    file.Position = 0;
                    await sw.WriteAsync(JsonSerializer.Serialize(tasks));
                });
            });
        }
    }
}

class Task
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public bool Done { get; set; } = false;
}