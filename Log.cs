using Haruka.Common.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;

namespace Haruka.Common;

public static class Log {
    public static ILogger Main { get; private set; }
    public static ILogger Conf { get; private set; }
    public static Dictionary<string, ILogger> Loggers { get; private set; }

    private static ILoggerFactory factory;

    public static void Initialize() {
        Loggers = new Dictionary<string, ILogger>();
        
        IConfigurationSection loggingConfig = AppConfig.Current.GetSection("Logging");

        factory = LoggerFactory.Create(builder => builder
            .AddConfiguration(loggingConfig)
            .AddSimpleConsole(options => { options.SingleLine = true; })
            .AddDebug()
            .AddFile(loggingConfig.GetSection("File"))
        );
        Main = factory.CreateLogger("Main");
        Conf = factory.CreateLogger("Conf");
        
        Main.LogInformation("Logging started.");
    }

    public static ILogger GetOrCreate(string key) {
        if (Loggers[key] == null) {
            Loggers[key] = factory.CreateLogger(key);
        }

        return Loggers[key];
    }
}