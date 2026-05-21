using System.ComponentModel;
using System.Runtime.InteropServices;
using Haruka.Common.Collections;
using Microsoft.Extensions.Logging;

namespace Haruka.Common.Configuration;

public class IniFile {
    public const string DEFAULT_SECTION = "Default";

    public string Path { get; }

    public static IniFile New(string iniPath) {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            Log.Conf.LogTrace("Using Windows accessor");
            return new IniFile(iniPath);
        }

        Log.Conf.LogTrace("Using custom parser");
        return new IniParser(iniPath);
    }

    protected IniFile(string iniPath) {
        Path = new FileInfo(iniPath).FullName;
        Log.Conf.LogInformation("Preparing " + Path);
    }

    public virtual string Read(string key, string section = null) {
        Log.Conf.LogDebug("Reading " + GetFileName() + ": " + (section != null ? "[" + section + "] " : "") + "" + key);

        if (section == null) {
            section = DEFAULT_SECTION;
        }

        char[] buf = new char[32];
        bool retry;
        do {
            buf.Fill('\0');
            int read = NativeMethods.GetPrivateProfileString(section, key, "", buf, buf.Length / 2, Path);
            if (read * 2 >= buf.Length - 3) {
                buf = new char[buf.Length * 2];
                retry = true;
            } else {
                retry = false;
            }
        } while (retry);

        string str = new string(buf, 0, buf.Length).Trim('\0');

        Log.Conf.LogDebug("Read Result: " + str);

        return str;
    }

    private string GetFileName() {
        return System.IO.Path.GetFileName(Path);
    }

    public virtual void Write(string key, string value, string section = null) {
        Log.Conf.LogInformation("Updating " + GetFileName() + ": " + (section != null ? "[" + section + "] " : "") + "" + key + " -> " + value);

        if (section == null) {
            section = DEFAULT_SECTION;
        }

        if (!NativeMethods.WritePrivateProfileString(section, key, value, Path)) {
            throw new IOException("Failed to write to " + Path, new Win32Exception());
        }
    }

    public void Write(string key, object value, string section = null) {
        Write(key, value?.ToString(), section);
    }

    public void DeleteKey(string key, string section = null) {
        Write(key, null, section);
    }

    public void DeleteSection(string section = null) {
        Write(null, null, section);
    }

    public bool KeyExists(string key, string section = null) {
        return (Read(key, section) ?? "").Length > 0;
    }

    public virtual List<string> GetSections() {
        Log.Conf.LogDebug("Reading " + GetFileName() + ": Querying sections", "Configuration");

        char[] buf = new char[32];
        bool retry;
        do {
            buf.Fill('\0');
            int read = NativeMethods.GetPrivateProfileSectionNames(buf, buf.Length, Path);
            if (read * 2 >= buf.Length - 3) {
                buf = new char[buf.Length * 2];
                retry = true;
            } else {
                retry = false;
            }
        } while (retry);

        string allSections = new string(buf, 0, buf.Length);
        string[] sectionNames = allSections.Split('\0');
        List<string> s = new List<string>();
        foreach (string sectionName in sectionNames) {
            if (sectionName != "") {
                s.Add(sectionName);
            }
        }

        return s;
    }

    public virtual List<string> GetKeys(string section) {
        Log.Conf.LogDebug("Reading " + GetFileName() + ": " + (section != null ? "[" + section + "] " : "") + "Querying keys");

        char[] buf = new char[32];
        bool retry;
        do {
            buf.Fill('\0');
            int read =
                NativeMethods.GetPrivateProfileSection(section, buf, buf.Length, Path);
            if (read * 2 >= buf.Length - 3) {
                buf = new char[buf.Length * 2];
                retry = true;
            } else {
                retry = false;
            }
        } while (retry);

        string[] tmp = new string(buf, 0, buf.Length).Trim('\0').Split('\0');

        List<string> result = new List<string>();

        foreach (string entry in tmp) {
            if (!entry.StartsWith('#') && !entry.StartsWith(';')) {
                int index = entry.IndexOf('=');
                if (index > -1) {
                    result.Add(entry.Substring(0, index));
                }
            }
        }

        return result;
    }

    public string ReadString(string key, string section = null, string def = null) {
        string s = Read(key, section);
        return String.IsNullOrEmpty(s) ? def : s;
    }

    public int ReadInt(string key, string section = null, int def = 0) {
        string s = Read(key, section);
        return Int32.TryParse(s, out int i) ? i : def;
    }

    public bool ReadBool(string key, string section = null, bool def = false) {
        string s = Read(key, section);
        return Boolean.TryParse(s, out bool b) ? b : def;
    }
}