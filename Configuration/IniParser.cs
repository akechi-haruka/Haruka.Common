using OAS.Util.Configuration;
using OAS.Util.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace OAS.Lib.Configuration {
    public class IniParser : IniFile {
        private Hashtable keyPairs = new Hashtable();
        private bool logging;

        private struct SectionPair {
            public String Section;
            public String Key;
        }

        /// <summary>
        /// Opens the INI file at the given path and enumerates the values in the IniParser.
        /// </summary>
        /// <param name="iniPath">Full path to INI file.</param>
        public IniParser(String iniPath) : base(iniPath) {
            TextReader iniFile = null;
            String strLine = null;
            String currentRoot = null;
            String[] keyPair = null;

            iniPath = path;

            if (File.Exists(iniPath)) {
                try {
                    iniFile = new StreamReader(iniPath);

                    strLine = iniFile.ReadLine();

                    while (strLine != null) {
                        strLine = strLine.Trim();

                        if (strLine != "") {
                            if (strLine.StartsWith("[") && strLine.EndsWith("]")) {
                                currentRoot = strLine.Substring(1, strLine.Length - 2);
                            } else {
                                keyPair = strLine.Split(new char[] { '=' }, 2);

                                SectionPair sectionPair;
                                String value = null;

                                if (currentRoot == null)
                                    currentRoot = "ROOT";

                                sectionPair.Section = currentRoot;
                                sectionPair.Key = keyPair[0];

                                if (keyPair.Length > 1)
                                    value = keyPair[1];

                                keyPairs[sectionPair] = value;
                            }
                        }

                        strLine = iniFile.ReadLine();
                    }

                } finally {
                    if (iniFile != null)
                        iniFile.Close();
                }
            }

            logging = ReadBool("EnableAccessLogging", "Ini", true);
        }

        public override string Read(string Key, string Section = null) {
            if (logging) {
                Log.Write("Reading " + path + ": " + (Section != null ? "[" + Section + "] " : "") + "" + Key, "Configuration");
            }
            SectionPair sectionPair;
            sectionPair.Section = Section;
            sectionPair.Key = Key;

            if (logging) {
                Log.Write("Read Result: " + keyPairs[sectionPair], "Configuration");
            }
            return (String)keyPairs[sectionPair];
        }

        public override List<string> GetKeys(string section = null) {
            ArrayList tmpArray = new ArrayList();

            foreach (SectionPair pair in keyPairs.Keys) {
                if (pair.Section == section)
                    tmpArray.Add(pair.Key);
            }

            return new List<string>((String[])tmpArray.ToArray(typeof(String)));
        }

        public override void Write(string Key, string Value, string Section = null) {
            SectionPair sectionPair;
            sectionPair.Section = Section;
            sectionPair.Key = Key;

            if (keyPairs.ContainsKey(sectionPair))
                keyPairs.Remove(sectionPair);

            keyPairs.Add(sectionPair, Value);
            SaveSettings();
        }

        /// <summary>
        /// Save settings to new file.
        /// </summary>
        /// <param name="newFilePath">New file path.</param>
        public void SaveSettings(String newFilePath) {
            ArrayList sections = new ArrayList();
            String tmpValue = "";
            String strToSave = "";

            foreach (SectionPair sectionPair in keyPairs.Keys) {
                if (!sections.Contains(sectionPair.Section))
                    sections.Add(sectionPair.Section);
            }

            foreach (String section in sections) {
                strToSave += ("[" + section + "]\r\n");

                foreach (SectionPair sectionPair in keyPairs.Keys) {
                    if (sectionPair.Section == section) {
                        tmpValue = (String)keyPairs[sectionPair];

                        if (tmpValue != null)
                            tmpValue = "=" + tmpValue;

                        strToSave += (sectionPair.Key + tmpValue + "\r\n");
                    }
                }

                strToSave += "\r\n";
            }

            TextWriter tw = new StreamWriter(newFilePath);
            tw.Write(strToSave);
            tw.Close();
        }

        /// <summary>
        /// Save settings back to ini file.
        /// </summary>
        public void SaveSettings() {
            SaveSettings(path);
        }
    }
}
