using System.Collections.Generic;
using System.IO;

namespace MonoGameRayTracer.Utils
{
    public sealed class ConfigParser
    {
        protected Dictionary<string, string> m_Parameters;

        public ConfigParser(string filePath)
        {
            m_Parameters = new Dictionary<string, string>();

            if (File.Exists(filePath))
            {
                var text = File.ReadAllText(filePath);
                Parse(text);
            }
        }

        public bool HasKey(string key) => m_Parameters.ContainsKey(key);

        public void GetInteger(string key, ref int property)
        {
            if (m_Parameters.ContainsKey(key))
            {
                var value = m_Parameters[key];
                int result;
                if (int.TryParse(value, out result))
                    property = result;
            }
        }

        public void GetFloat(string key, ref float property)
        {
            if (m_Parameters.ContainsKey(key))
            {
                var value = m_Parameters[key];
                float result;
                if (float.TryParse(value, out result))
                    property = result;
            }
        }

        public void GetBool(string key, ref bool property)
        {
            if (m_Parameters.ContainsKey(key))
                property = m_Parameters[key].ToLower() == "true";
        }

        public void GetString(string key, ref string property)
        {
            if (m_Parameters.ContainsKey(key))
                property = m_Parameters[key];
        }

        private void Parse(string data)
        {
            using (var stream = new StringReader(data))
            {
                var line = stream.ReadLine();
                var temp = new string[2];
                var key = string.Empty;
                var value = string.Empty;

                while (line != null)
                {
                    temp = line.Split('=');

                    if (temp.Length == 2)
                    {
                        key = temp[0].Trim();
                        value = temp[1].Trim();

                        if (key.Contains(";") || value == string.Empty)
                            continue;

                        value = temp[1].Trim();

                        if (m_Parameters.ContainsKey(key))
                            m_Parameters[key] = value;
                        else
                            m_Parameters.Add(key, value);
                    }

                    line = stream.ReadLine();
                }
            }
        }
    }
}
