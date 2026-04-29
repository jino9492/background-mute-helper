using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BackgroundMuteHelper
{
    public partial class Mixer
    {
        private static readonly object settingLock = new object();

        private static string SettingPath
        {
            get { return Path.Combine(Application.StartupPath, "setting.json"); }
        }

        public static List<string> GetProgramList()
        {
            lock (settingLock)
            {
                return new List<string>(programList);
            }
        }

        public static void SaveProgramList(IEnumerable<string> programs)
        {
            List<string> normalized = programs
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            lock (settingLock)
            {
                JObject root;
                string path = SettingPath;
                if (File.Exists(path))
                {
                    root = JObject.Parse(File.ReadAllText(path));
                }
                else
                {
                    root = new JObject();
                    root["icon"] = "main.ico";
                }

                root["program"] = JArray.FromObject(normalized);
                File.WriteAllText(path, root.ToString(Formatting.Indented));

                programArray = (JArray)root["program"];
                programList = new List<string>(normalized);
            }

            try
            {
                Mixer.target = GetTargetProgram();
            }
            catch
            {
            }
        }

        public static List<string> GetRunningAudioProcessNames()
        {
            HashSet<string> names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
                MMDevice defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                SessionCollection sessions = defaultDevice.AudioSessionManager.Sessions;

                for (int i = 0; i < sessions.Count; i++)
                {
                    try
                    {
                        int pid = (int)sessions[i].GetProcessID;
                        if (pid <= 0)
                        {
                            continue;
                        }

                        Process process = Process.GetProcessById(pid);
                        if (!string.IsNullOrEmpty(process.ProcessName))
                        {
                            names.Add(process.ProcessName);
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            catch
            {
            }

            return names.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}
