using Microsoft.Extensions.Configuration;

namespace Haruka.Common.Configuration;

class AppConfig {
    public static IConfigurationRoot Current { get; private set; }

    public static void Initialize() {
        Current = new ConfigurationBuilder()
            .AddJsonFile("Haruka.Common.Settings.json", false)
            .AddJsonFile("Haruka.Common.Settings.debug.json", true, true)
            .AddJsonFile("Haruka.Common.Settings.local.json", true, true)
            .Build();
    }

    public static string Get(string section, string value) {
        return Current.GetSection(section).GetSection(value)?.Value;
    }

    public static string Get(string section, string subsection, string value) {
        return Current.GetSection(section).GetSection(subsection).GetSection(value).Value;
    }

    public static int GetInt(string section, string value) {
        return Current.GetSection(section).GetValue<int>(value);
    }

    public static bool GetBool(string section, string value) {
        return Current.GetSection(section).GetValue<bool>(value);
    }

    public static bool GetBool(string section, string subsection, string value) {
        return Current.GetSection(section).GetSection(subsection).GetValue<bool>(value);
    }
}