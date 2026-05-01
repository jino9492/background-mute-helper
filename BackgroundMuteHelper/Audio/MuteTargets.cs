using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BackgroundMuteHelper
{
    internal static class MuteTargets
    {
        private static readonly object syncLock = new object();
        private static List<string> programList = LoadInitial();
        private static HashSet<string> programSet = BuildSet(programList);

        private static List<string> LoadInitial()
        {
            try
            {
                JObject root = JObject.Parse(EmbeddedAssets.ReadSettingJson());
                JArray arr = root["program"] as JArray;
                if (arr != null)
                {
                    return arr.ToObject<List<string>>() ?? new List<string>();
                }
            }
            catch
            {
            }
            return new List<string>();
        }

        private static HashSet<string> BuildSet(IEnumerable<string> names)
        {
            return new HashSet<string>(names ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        }

        public static List<string> GetProgramList()
        {
            lock (syncLock)
            {
                return new List<string>(programList);
            }
        }

        public static HashSet<string> GetProgramSet()
        {
            lock (syncLock)
            {
                return new HashSet<string>(programSet, StringComparer.OrdinalIgnoreCase);
            }
        }

        public static void Save(IEnumerable<string> programs)
        {
            List<string> normalized = programs
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            lock (syncLock)
            {
                JObject root = JObject.Parse(EmbeddedAssets.ReadSettingJson());
                root["program"] = JArray.FromObject(normalized);
                EmbeddedAssets.EnsureSettingDirectory();
                File.WriteAllText(EmbeddedAssets.SettingFilePath, root.ToString(Formatting.Indented));

                programList = new List<string>(normalized);
                programSet = BuildSet(programList);
            }
        }
    }
}
