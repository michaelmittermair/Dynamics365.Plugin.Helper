using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Dynamics.Plugin.Helper
{
    /// <summary>
    /// Providing the nessesary functions to provide the information from the plugin configuration
    /// </summary>
    public class PluginConfiguration
    {
        public readonly Dictionary<string, string> Map;

        public PluginConfiguration(string input)
        {
            try
            {
                var doc = XDocument.Parse(input);
                Map = GetMap(doc);
            }
            catch (Exception)
            {
                Map = new Dictionary<string, string>();
            }
        }


        private static Dictionary<string, string> GetMap(XDocument doc)
        {
            if (doc.Root == null) return new Dictionary<string, string>();

            try
            {
                return doc.Root.Elements()
                    .ToDictionary(a => (string)a.Attribute("key"),
                        a => (string)a.Attribute("value"));
            }
            catch (Exception)
            {
                return new Dictionary<string, string>();
            }
        }

        private string GetValueNode(string key)
        {
            return Map.ContainsKey(key) ? Map.First(x => x.Key == key).Value : string.Empty;
        }

        public Guid GetConfigDataGuid(string key)
        {
            return Map.ContainsKey(key) ? new Guid(Map.First(x => x.Key == key).Value) : Guid.Empty;
        }

        public bool GetConfigDataBool(string key)
        {
            return bool.TryParse(GetValueNode(key), out bool retVar) && retVar;
        }

        public int GetConfigDataInt(string key)
        {
            if (int.TryParse(GetValueNode(key), out int retVar))
                return retVar;
            return -1;
        }

        public string GetConfigDataString(string key)
        {
            return GetValueNode(key);
        }

        public double GetConfigDataDouble(string key)
        {
            if (double.TryParse(GetValueNode(key), NumberStyles.Any, CultureInfo.InvariantCulture, out double retVal))
                return retVal;

            return -1;
        }
    }
}