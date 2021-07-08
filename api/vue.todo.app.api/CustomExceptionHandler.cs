using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

static class CustomExceptionHandler
{
    public static void UseCustomExceptionHandler(this IApplicationBuilder app)
    {
        app.Use(Run);
    }

    private static async System.Threading.Tasks.Task Run(HttpContext context, Func<System.Threading.Tasks.Task> next)
    {
        try
        {
            await next.Invoke();
        }
        catch (BadHttpRequestException ex)
        {
            context.Response.Clear();
            context.Response.ContentType = "application/json";            
            context.Response.StatusCode = ex.StatusCode;

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                message = ex.Message
            }), Encoding.UTF8);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            Console.WriteLine($"{ex.GetType().Name} {ex.Message}");
        }
    }
}