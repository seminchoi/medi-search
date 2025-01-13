using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace HourDataProcessor.utils;

public static class LoggerHelper
{
    private static ILogger _logger;

    static LoggerHelper()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true)
            .CreateLogger();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(configure => { configure.AddSerilog(Log.Logger); });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        _logger = serviceProvider.GetService<ILogger<Program>>();
    }

    public static void LogInfo(string message) => _logger.LogInformation(message);
    public static void LogWarning(string message) => _logger.LogWarning(message);
    public static void LogError(string message) => _logger.LogError(message);
    public static void LogError(Exception ex, string message) => _logger.LogError(ex, message);
}