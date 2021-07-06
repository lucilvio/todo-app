using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Vue.TodoApp
{
    static class TasksApi
    {
        private static string FileName = "tasks.json";

        public static void Map(IEndpointRouteBuilder router)
        {
            router.MapGet("/tasks", Get);
            router.MapPost("/tasks", Post);
            router.MapPut("/tasks/{id}/done", PutDone);
            router.MapPut("/tasks/{id}/undo", PutUndo);
            router.MapDelete("/tasks/{id}", Delete);
        }

        private static async System.Threading.Tasks.Task Get(HttpContext context)
        {
            using var fileToRead = File.Open(FileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);

            IList<Task> tasks = new List<Task>();

            using var sr = new StreamReader(fileToRead);

            if (fileToRead.Length > 0)
                tasks = JsonSerializer.Deserialize<IList<Task>>(await sr.ReadToEndAsync());

            await context.Response.WriteAsJsonAsync(tasks);
        }

        private static async System.Threading.Tasks.Task Post(HttpContext context)
        {
            if (context.Request.ContentLength <= 0)
                throw new BadHttpRequestException("Please, inform task data");

            using var requestReader = new StreamReader(context.Request.Body);
            var jsonRequest = await requestReader.ReadToEndAsync();

            var task = JsonSerializer.Deserialize<Task>(jsonRequest);

            if (task is null || !task.Valid)
                throw new BadHttpRequestException("Task must have a name");

            using var fileToRead = File.Open(FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            using var sr = new StreamReader(fileToRead);

            IList<Task> tasks = new List<Task>();

            if (fileToRead.Length > 0)
                tasks = JsonSerializer.Deserialize<IList<Task>>(await sr.ReadToEndAsync());

            if(tasks.Any(t => t.Name == task.Name))
                throw new BadHttpRequestException("Can't register tasks with same name");

            tasks.Add(task);

            using var sw = new StreamWriter(fileToRead);
            fileToRead.Position = 0;
            await sw.WriteAsync(JsonSerializer.Serialize(tasks));
        }

        private static async System.Threading.Tasks.Task PutDone(HttpContext context)
        {
            var requestData = context.GetRouteData();

            var requestTaskId = requestData.Values["id"];

            if(requestTaskId is null)
                throw new BadHttpRequestException("Task Id not informed");

            if(!Guid.TryParse(requestTaskId.ToString(), out var taskId))
                throw new BadHttpRequestException("Task Id malformed");

            using var fileToRead = File.Open(FileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
            using var sr = new StreamReader(fileToRead);

            IList<Task> tasks = new List<Task>();
            
            if (fileToRead.Length > 0)
                tasks = JsonSerializer.Deserialize<IList<Task>>(await sr.ReadToEndAsync());

            var foundTask = tasks.FirstOrDefault(t => t.Id == taskId);

            if(foundTask is null)
                throw new BadHttpRequestException("Task not found", (int)HttpStatusCode.NotFound);

            foundTask.MarkAsDone();

            fileToRead.Close();

            using var fileToWrite = File.Open(FileName, FileMode.Truncate, FileAccess.Write, FileShare.Read);            
            using var sw = new StreamWriter(fileToWrite);
            Console.WriteLine(JsonSerializer.Serialize(tasks));
            await sw.WriteAsync(JsonSerializer.Serialize(tasks));
        }

        private static async System.Threading.Tasks.Task PutUndo(HttpContext context)
        {
            var requestData = context.GetRouteData();

            var requestTaskId = requestData.Values["id"];

            if(requestTaskId is null)
                throw new BadHttpRequestException("Task Id not informed");

            if(!Guid.TryParse(requestTaskId.ToString(), out var taskId))
                throw new BadHttpRequestException("Task Id malformed");

            using var fileToRead = File.Open(FileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
            using var sr = new StreamReader(fileToRead);

            IList<Task> tasks = new List<Task>();
            
            if (fileToRead.Length > 0)
                tasks = JsonSerializer.Deserialize<IList<Task>>(await sr.ReadToEndAsync());

            var foundTask = tasks.FirstOrDefault(t => t.Id == taskId);

            if(foundTask is null)
                throw new BadHttpRequestException("Task not found", (int)HttpStatusCode.NotFound);

            foundTask.MarkAsTodo();

            fileToRead.Close();

            using var fileToWrite = File.Open(FileName, FileMode.Truncate, FileAccess.Write, FileShare.Read);            
            using var sw = new StreamWriter(fileToWrite);
            Console.WriteLine(JsonSerializer.Serialize(tasks));
            await sw.WriteAsync(JsonSerializer.Serialize(tasks));
        }

        private static async System.Threading.Tasks.Task Delete(HttpContext context)
        {
            var requestData = context.GetRouteData();

            var requestTaskId = requestData.Values["id"];

            if(requestTaskId is null)
                throw new BadHttpRequestException("Task Id not informed");

            if(!Guid.TryParse(requestTaskId.ToString(), out var taskId))
                throw new BadHttpRequestException("Task Id malformed");

            using var fileToRead = File.Open(FileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
            using var sr = new StreamReader(fileToRead);

            IList<Task> tasks = new List<Task>();
            
            if (fileToRead.Length > 0)
                tasks = JsonSerializer.Deserialize<IList<Task>>(await sr.ReadToEndAsync());

            var foundTask = tasks.FirstOrDefault(t => t.Id == taskId);

            if(foundTask is null)
                throw new BadHttpRequestException("Task not found", (int)HttpStatusCode.NotFound);

            tasks = tasks.Where(t => t.Id != taskId).ToList();

            fileToRead.Close();

            using var fileToWrite = File.Open(FileName, FileMode.Truncate, FileAccess.Write, FileShare.Read);            
            using var sw = new StreamWriter(fileToWrite);
            await sw.WriteAsync(JsonSerializer.Serialize(tasks));
        }
    }

    class Task
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public bool Done { get; set; } = false;

        [JsonIgnore]
        public bool Valid => !string.IsNullOrEmpty(this.Name);

        internal void MarkAsDone()
        {
            this.Done = true;
        }

        internal void MarkAsTodo()
        {
            this.Done = false;
        }
    }
}