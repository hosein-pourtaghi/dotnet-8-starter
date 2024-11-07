using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.MSSqlServer;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using static Serilog.Sinks.MSSqlServer.ColumnOptions;
using System;

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
                     tableName: "Logs",
                     autoCreateSqlTable:true
              //       , sinkOptions: new MSSqlServerSinkOptions {
              //                   TableName="Applogs",
              //AutoCreateSqlTable=true,
              //           SchemaName ="Logger", 
              //       }
                    //schemaName: "Logging" 
                    //,autoCreateTable: true     // Automatically create the table



                       
            //MSSqlServerSinkOptions sinkOptions = null,
            //IConfigurationSection sinkOptionsSection = null,
            //IConfiguration appConfiguration = null,
            //LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            //IFormatProvider formatProvider = null,
            //ColumnOptions columnOptions = null,
            //IConfigurationSection columnOptionsSection = null,
            //ITextFormatter logEventFormatter = null




                    )
                .CreateLogger();


        }
        public static void UseSerilogMiddleware(this WebApplication app)
        {
            app.UseMiddleware<LoggingMiddleware>();
        }


    }
}
