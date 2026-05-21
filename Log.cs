using Haruka.Common.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;

namespace Haruka.Common;

public static class Log {
    public static ILogger Main { get; private set; }
    public static ILogger Conf { get; private set; }
    public static Dictionary<string, ILogger> Loggers { get; private set; }
    public static ILoggerFactory Factory { get; private set; }

    public static void Initialize(bool silent = false, bool singleLine = true) {
        Loggers = new Dictionary<string, ILogger>();

        IConfigurationSection loggingConfig = AppConfig.Primary.GetSection("Logging");

        Factory = LoggerFactory.Create(builder => {
            builder
                .AddConfiguration(loggingConfig)
                .AddDebug()
                .AddFile(loggingConfig.GetSection("File"), opts => { opts.HandleFileError = (err) => { err.UseNewLogFileName(err.LogFileName + "_" + DateTime.Now.Ticks); }; });
            if (!silent) {
                builder.AddSimpleConsole(options => { options.SingleLine = singleLine; });
            }
        });
        Main = GetOrCreate("Main");
        Conf = GetOrCreate("Conf");

        Main.LogInformation("Logging started.");
    }

    public static ILogger GetOrCreate(string key) {
        if (Loggers.TryGetValue(key, out ILogger value)) {
            return value;
        }

        value = Factory.CreateLogger(key);
        Loggers[key] = value;

        return value;
    }

    public static void FlushAndDispose() {
        factory.Dispose();
    }
}