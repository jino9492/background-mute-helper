using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BackgroundMuteHelper
{
    internal static class AppPreferences
    {
        private static readonly object syncLock = new object();

        public static bool GetAutoOpenGui()
        {
            lock (syncLock)
            {
                try
                {
                    JObject root = JObject.Parse(EmbeddedAssets.ReadSettingJson());
                    JToken token = root["autoOpenGui"];
                    if (token != null && token.Type == JTokenType.Boolean)
                    {
                        return (bool)token;
                    }
                }
                catch
                {
                }
                return false;
            }
        }

        public static void SetAutoOpenGui(bool value)
        {
            lock (syncLock)
            {
                JObject root = JObject.Parse(EmbeddedAssets.ReadSettingJson());
                root["autoOpenGui"] = value;
                EmbeddedAssets.EnsureSettingDirectory();
                File.WriteAllText(EmbeddedAssets.SettingFilePath, root.ToString(Formatting.Indented));
            }
        }
    }
}
