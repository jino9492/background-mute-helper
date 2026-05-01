using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NAudio.CoreAudioApi;

namespace BackgroundMuteHelper
{
    internal static class AudioSessionInspector
    {
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
