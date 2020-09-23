using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AmongUsCapture
{
    class Config
    {

        private static Config instance = null;
        private const string cfgFile = "config.txt";

        private Dictionary<string, string> variables;

        public static Config GetInstance()
        {
            if (instance == null) instance = new Config();
            return instance;
        }

        private Config()
        {
            variables = new Dictionary<string, string>();

            readConfig();
        }

        public string Get(string key)
        {
            key = key.ToLowerInvariant();
            if (variables.ContainsKey(key)) return variables.GetValueOrDefault(key);
            else throw new ArgumentException("Unknown key");
        }

        public string GetOrDefault(string key, string defaultResult)
        {
            key = key.ToLowerInvariant();
            return (variables.ContainsKey(key)) ? variables.GetValueOrDefault(key) : defaultResult;
        }

        public void Set(string key, string value)
        {
            var write = false;
            if (!variables.ContainsKey(key) || variables.GetValueOrDefault(key) != value)
            {
                write = true;
            }

            variables[key] = value;

            if (write) writeConfig();
        }

        public void readConfig()
        {
            if (File.Exists(cfgFile))
            {
                var lines = File.ReadAllLines(cfgFile);
                variables.Clear();
                foreach (var line in lines)
                {
                    var separator = line.IndexOf("=");
                    if (separator == -1) continue;
                    var key = line.Substring(0, separator).Trim().ToLowerInvariant();
                    var val = line.Substring(separator + 1).Trim();

                    variables[key] = val;
                }
            }
        }

        public void writeConfig()
        {
            var lines = new List<string>();
            foreach (var key in variables.Keys) {
                lines.Add(key + " = " + variables.GetValueOrDefault(key));
            }
            File.WriteAllLines(cfgFile, lines);
        }
    }
}
