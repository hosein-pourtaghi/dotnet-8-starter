using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Threading.Tasks;
using Serilog;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;

    public LoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        // Log request details
        Log.Information("Handling request: {Method} {Path}", context.Request.Method, context.Request.Path);
        
        // Call the next middleware in the pipeline
        await _next(context);

        // Log response details
        stopwatch.Stop();
        Log.Information("Finished handling request. Status Code: {StatusCode}, Time: {ElapsedMilliseconds}ms",
            context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
    }
}
