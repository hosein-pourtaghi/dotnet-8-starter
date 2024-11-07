using Microsoft.AspNetCore.Http;
using Serilog;
using System.Diagnostics;
using System.Text;

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
        var requestBody = await ReadRequestBodyAsync(context);
        var queryParams = context.Request.QueryString.ToString();
        var requestHeaders = context.Request.Headers.ToString();

        // Log request details
        Log.Information("Handling request: {Method} {Path} {QueryParams} {RequestHeaders} {RequestBody}",
            context.Request.Method, context.Request.Path, queryParams, requestHeaders, requestBody);

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

            Log.Information("Finished handling request. Status Code: {StatusCode}, Time: {ElapsedMilliseconds}ms, ResponseHeaders: {ResponseHeaders}, ResponseBody: {ResponseBody}",
                context.Response.StatusCode, stopwatch.ElapsedMilliseconds, responseHeaders, responseBodyText);

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
