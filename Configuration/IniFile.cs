using OAS.Lib.Configuration;
using OAS.Util.CodeHelpers;
using OAS.Util.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace OAS.Util.Configuration {

    public class IniFile {

        public const string SECTION_GENERAL = "General";
        public const string SETTING_LANGUAGE = "Language";
        public const string SECTION_UPDATE = "Update";
        public const string SECTION_INTEGRATION = "Integration";
        public const string SECTION_BUTTONS = "Buttons";
        public const string SECTION_GAMELIST = "ApplicationList";
        public const string SECTION_SOUND = "Sound";
        public const string SECTION_DEVICELOADER = "DeviceLoader";
        public const string SECTION_CONFIGFILES = "ConfigFiles";
        public const string SECTION_USERUI = "UserUI";
        public const string SECTION_CARD = "Card";
        public const string SECTION_PRINTER = "Printer";

        public Func<IniFile> ConfigurationProvider { get; set; }

        protected string path;
        string exe;
        IniFile overrideFile;

        private StringBuilder daBuffa = new StringBuilder(65535 * 100);
        private byte[] daBuffaForSections = new byte[65535 * 100];
        private bool logging;

        public static IniFile New(string iniPath = null) {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                Log.WriteTraced("Using Windows accessor");
                return new IniFile(iniPath);
            } else {
                Log.WriteTraced("Using custom parser");
                return new IniParser(iniPath);
            }
        }

        public IniFile(string iniPath = null) {
            try {
                exe = Assembly.GetEntryAssembly()?.GetName()?.Name ?? "UnknownOASApplication";
            } catch { }
            path = new FileInfo(iniPath ?? exe + ".ini").FullName;
            Log.Write("Preparing " + path, "Configuration");

            if (ReadBool("AllowOverride", "Ini")) {
                overrideFile = IniFile.New((iniPath ?? exe + ".ini") + ".Override.ini");
            }

            logging = ReadBool("EnableAccessLogging", "Ini", true);
        }

        public IniFile(FileInfo config) : this(config?.FullName) {
        }

        public String GetPath() {
            return path;
        }

        public virtual string Read(string key, string section = null) {
            if (logging) {
                Log.Write("Reading " + GetFileName() + ": " + (section != null ? "[" + section + "] " : "") + "" + key, "Configuration");
            }
            daBuffa.Clear();
            if (overrideFile != null) {
                string overrideValue = overrideFile.Read(key, section);
                if (overrideValue != String.Empty) {
                    return overrideValue;
                }
            }
            NativeMethods.GetPrivateProfileString(section ?? exe, key, String.Empty, daBuffa, daBuffa.MaxCapacity, path);
            if (logging) {
                Log.Write("Read Result: " + daBuffa, "Configuration");
            }
            return daBuffa.ToString();
        }

        private string GetFileName() {
            return System.IO.Path.GetFileName(path);
        }

        public virtual void Write(string key, string value, string section = null) {
            if (logging) {
                Log.Write("Updating " + GetFileName() + ": " + (section != null ? "[" + section + "] " : "") + "" + key + " -> " + value, "Configuration");
            }
            NativeMethods.WritePrivateProfileString(section ?? exe, key, value, path);
        }

        public void Write(string key, object value, string section = null) {
            Write(key, value?.ToString(), section);
        }

        public void DeleteKey(string key, string section = null) {
            Write(key, null, section ?? exe);
        }

        public void DeleteSection(string section = null) {
            Write(null, null, section ?? exe);
        }

        public bool KeyExists(string key, string section = null) {
            return (Read(key, section) ?? "").Length > 0;
        }

        public virtual List<string> GetSections() {
            if (logging) {
                Log.Write("Reading " + GetFileName() + ": Querying sections", "Configuration");
            }
            daBuffaForSections.Fill<byte>(0);
            NativeMethods.GetPrivateProfileSectionNames(daBuffaForSections, daBuffaForSections.Length, path);
            string allSections = Encoding.ASCII.GetString(daBuffaForSections);
            string[] sectionNames = allSections.Split('\0');
            List<string> s = new List<string>();
            foreach (string sectionName in sectionNames) {
                if (sectionName != string.Empty) {
                    s.Add(sectionName);
                }
            }
            return s;
        }

        public virtual List<string> GetKeys(string section = null) {
            if (logging) {
                Log.Write("Reading " + GetFileName() + ": " + (section != null ? "[" + section + "] " : "") + "Querying keys", "Configuration");
            }
            daBuffaForSections.Fill<byte>(0);
            NativeMethods.GetPrivateProfileSection(section, daBuffaForSections, daBuffaForSections.Length, path);
            string[] tmp = Encoding.ASCII.GetString(daBuffaForSections).Trim('\0').Split('\0');

            List<string> result = new List<string>();

            foreach (string entry in tmp) {
                if (!entry.StartsWith("#") && !entry.StartsWith(";")) {
                    try {
                        result.Add(entry.Substring(0, entry.IndexOf("=")));
                    } catch { }
                }
            }

            return result;
        }

        public string GetHashOfContents() {
            return Crypto.BytesToString(Crypto.HashFile(path));
        }

        public string ReadString(string key, string section = null, string def = null) {
            string s = Read(key, section);
            return String.IsNullOrEmpty(s) ? def : s;
        }

        public int ReadInt(string key, string section = null, int def = 0) {
            string s = Read(key, section);
            if (Int32.TryParse(s, out int i)) {
                return i;
            } else {
                return def;
            }
        }

        public bool ReadBool(string key, string section = null, bool def = false) {
            string s = Read(key, section);
            if (Boolean.TryParse(s, out bool b)) {
                return b;
            } else {
                return def;
            }
        }
    }
}