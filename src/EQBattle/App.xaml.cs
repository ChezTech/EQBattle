using EQBattle.ViewModels;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Windows;

namespace EQBattle
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const int FileLimitBytes = 1024 * 1024 * 100;// 100 MiB

        public static IConfigurationRoot Configuration { get; private set; }
        public static AppSettings AppSettings { get; private set; }
        public static LogSettings LogSettings { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            LoadAppSettings(e.Args);
            SetupLogger();
            Log.Information("==============================================================");
            DumpSettings();

            var app = new MainWindow();
            app.DataContext = new MainWindowViewModel();
            app.Show();
        }

        private static void DumpSettings()
        {
            if (!Log.IsEnabled(Serilog.Events.LogEventLevel.Verbose))
                return;

            Log.Verbose(AppSettings.ToJson());
            Log.Verbose(LogSettings.ToJson());
        }

        private static void LoadAppSettings(string[] args)
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddCommandLine(args);

            Configuration = configBuilder
                .Build();

            AppSettings = new AppSettings();
            Configuration.Bind(AppSettings);

            LogSettings = new LogSettings();
            Configuration.GetSection("Logging").Bind(LogSettings);
        }

        private static void SetupLogger()
        {
            var logConfig = new LoggerConfiguration()
                .MinimumLevel(LogSettings.LogLevel)
                .Enrich.WithThreadId()
                .WriteTo.File(LogSettings.LogFileName,
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: FileLimitBytes,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] ({ThreadId}) {Message:lj}{NewLine}{Exception}")
                .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] ({ThreadId}) {Message:lj}{NewLine}{Exception}");

            Log.Logger = logConfig.CreateLogger();
        }
    }

    public static class SerilogExtensions
    {
        public static LoggerConfiguration MinimumLevel(this LoggerConfiguration logConfig, string logLevel)
        {
            // Serilog.Events.LogEventLevel
            // Microsoft.Extensions.Logging.LogLevel
            switch (logLevel.ToLowerInvariant())
            {
                case "verbose": // Serilog
                case "trace": // Microsoft
                    return logConfig.MinimumLevel.Verbose();

                case "debug":
                    return logConfig.MinimumLevel.Debug();

                case "information":
                case "info":
                    return logConfig.MinimumLevel.Information();

                case "warning":
                case "warn":
                default:
                    return logConfig.MinimumLevel.Warning();

                case "error":
                case "err":
                    return logConfig.MinimumLevel.Error();

                case "fatal": // Serilog
                case "critical": // Microsoft
                    return logConfig.MinimumLevel.Fatal();

                case "none": // Microsoft
                    return logConfig.MinimumLevel.Fatal(); // Not an exact interpretation. Not sure how to disable Serilog at this point
            }
        }
    }

    public static class MyExtensions
    {
        public static string ToJson(this Object obj)
        {
            var stream1 = new MemoryStream();
            var ser = new DataContractJsonSerializer(obj.GetType());
            ser.WriteObject(stream1, obj);
            stream1.Position = 0;
            var sr = new StreamReader(stream1);
            var json = sr.ReadToEnd();
            return json;
        }
    }

    public class LogSettings
    {
        private string _logFileName;

        public string LogLevel { get; set; } = "Warning";

        public string LogFileName
        {
            get
            {
                return (string.IsNullOrEmpty(_logFileName))
                    ? $"{System.AppDomain.CurrentDomain.FriendlyName}.log"
                    : _logFileName;
            }
            set => _logFileName = value;
        }
    }

    public class AppSettings
    {
        public string EQLog { get; set; }
        public string Mode { get; set; } = "mo";
        public int ParserCount { get; set; } = 1;
    }
}
