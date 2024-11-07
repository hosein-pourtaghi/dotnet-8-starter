using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.IO;
using System.Text;
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

        // Capture the request body
        var requestBody = await ReadRequestBodyAsync(context);
        var queryParams = context.Request.QueryString.ToString();
        var requestHeaders = context.Request.Headers.ToString();

        // Enrich log with request details
        Log.ForContext("RequestBody", requestBody)
           .ForContext("RequestHeaders", requestHeaders)
           .ForContext("QueryParams", queryParams)
           .Information("Handling request: {Method} {Path}", context.Request.Method, context.Request.Path);

        // Capture the original response body stream
        var originalResponseBodyStream = context.Response.Body;

        using (var responseBody = new MemoryStream())
        {
            context.Response.Body = responseBody;

            // Call the next middleware in the pipeline
            await _next(context);

            // Log response details
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            var responseHeaders = context.Response.Headers.ToString();
            stopwatch.Stop();

            // Enrich log with response details
            Log.ForContext("ResponseBody", responseBodyText)
               .ForContext("ResponseHeaders", responseHeaders)
               .Information("Finished handling request. Status Code: {StatusCode}, Time: {ElapsedMilliseconds}ms",
                   context.Response.StatusCode, stopwatch.ElapsedMilliseconds);

            // Copy the contents of the new memory stream (responseBody) to the original stream
            await responseBody.CopyToAsync(originalResponseBodyStream);
        }
    }

    private async Task<string> ReadRequestBodyAsync(HttpContext context)
    {
        context.Request.EnableBuffering(); // Allow the request body to be read multiple times
        using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0; // Reset the request body stream position
            return body;
        }
    }
}
