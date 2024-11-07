using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace LoggingLibrary
{
    public static class Logging
    {
        public static void AddSerilog(IConfiguration configuration)
        {

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.MSSqlServer(
                    connectionString: configuration.GetConnectionString("DefaultConnection"),
                    tableName: "MyLogs",
                    autoCreateSqlTable: true,
                    schemaName: "Logging"
                    )
                .CreateLogger();


        }
        public static void UseSerilogMiddleware(this WebApplication app)
        {
            app.UseMiddleware<LoggingMiddleware>();
        }


    }
}
