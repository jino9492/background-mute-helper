using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Diagnostics;
using NAudio.CoreAudioApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Linq;


namespace BackgroundMuteHelper
{

    public partial class Mixer
    {
        #region Settings state
        static string settingJson = EmbeddedAssets.ReadSettingJson();
        static dynamic jsonObject = JsonConvert.DeserializeObject(settingJson);
        static JArray programArray = (jsonObject["program"] as JArray) ?? new JArray();
        static List<string> programList = programArray.ToObject<List<string>>() ?? new List<string>();
        static HashSet<string> programSet = BuildProgramSet(programList);

        private static HashSet<string> BuildProgramSet(IEnumerable<string> programs)
        {
            return new HashSet<string>(programs ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        }
        #endregion

        #region P/Invoke
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();
        #endregion

        #region Target Discovery
        public static Dictionary<int, AudioSessionControl> GetTargetProgram()
        {
            var result = new Dictionary<int, AudioSessionControl>();
            try
            {
                var enumerator = new MMDeviceEnumerator();
                var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                var sessions = defaultDevice.AudioSessionManager.Sessions;

                for (int i = 0; i < sessions.Count; i++)
                {
                    var session = sessions[i];
                    int pid;
                    string name;
                    try
                    {
                        pid = (int)session.GetProcessID;
                        if (pid <= 0) continue;
                        using (var p = Process.GetProcessById(pid))
                        {
                            name = p.ProcessName;
                        }
                    }
                    catch
                    {
                        continue;
                    }

                    if (programSet.Contains(name))
                    {
                        result[pid] = session;
                    }
                }
            }
            catch
            {
                // Swallow — caller retries on next rescan tick.
            }
            return result;
        }

        public static Dictionary<int, AudioSessionControl> target { get; set; } = GetTargetProgram();
        #endregion

        #region Mute Application
        public static void OnForegroundChanged(IntPtr foregroundHwnd)
        {
            uint fgPid = 0;
            if (foregroundHwnd != IntPtr.Zero)
            {
                GetWindowThreadProcessId(foregroundHwnd, out fgPid);
            }
            int fgPidInt = (int)fgPid;

            var snapshot = target;
            if (snapshot == null) return;

            foreach (var kv in snapshot)
            {
                bool shouldMute = kv.Key != fgPidInt;
                try
                {
                    var vol = kv.Value.SimpleAudioVolume;
                    if (vol.Mute != shouldMute)
                    {
                        vol.Mute = shouldMute;
                    }
                }
                catch
                {
                    // Session may have died; will be cleaned up on next rescan.
                }
            }
        }

        public static void ApplyMuteForCurrentForeground()
        {
            OnForegroundChanged(GetForegroundWindow());
        }
        #endregion
    }

    public partial class BackgroundMuteHelper : Form
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80;
                return cp;
            }
        }

        static string settingJson = EmbeddedAssets.ReadSettingJson();
        static dynamic jsonObject = JsonConvert.DeserializeObject(settingJson);

        private ContextMenu contextMenu;
        private NotifyIcon notifyIcon;

        #region Foreground hook
        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax,
            IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc,
            uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType,
            IntPtr hwnd, int idObject, int idChild,
            uint dwEventThread, uint dwmsEventTime);

        private const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
        private const uint WINEVENT_OUTOFCONTEXT = 0;

        private WinEventDelegate winEventProc;
        private IntPtr winEventHook;
        private Timer sessionRescanTimer;
        #endregion

        public BackgroundMuteHelper()
        {
            contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add(new MenuItem("설정 열기", new EventHandler((sender, e) =>
            {
                OpenSettingsForm();
            })));
            contextMenu.MenuItems.Add(new MenuItem("Exit", new EventHandler((sender, e) =>
            {
                ExitApplication();
            })));

            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = EmbeddedAssets.LoadIcon((string)jsonObject["icon"]);
            notifyIcon.ContextMenu = contextMenu;
            notifyIcon.DoubleClick += new EventHandler((sender, e) => OpenSettingsForm());
            notifyIcon.Visible = true;

            this.BackColor = Color.Magenta;
            this.TransparencyKey = Color.Magenta;
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.ShowInTaskbar = false;

            Mixer.ApplyMuteForCurrentForeground();

            // Push-based foreground change notification — zero CPU between switches.
            // OUTOFCONTEXT delivers callbacks via the registering thread's message
            // loop, so this runs on the UI thread without explicit Invoke.
            winEventProc = OnForegroundEvent;
            winEventHook = SetWinEventHook(
                EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND,
                IntPtr.Zero, winEventProc, 0, 0, WINEVENT_OUTOFCONTEXT);

            // Rescan sessions to catch newly launched / exited audio apps.
            sessionRescanTimer = new Timer { Interval = 5000 };
            sessionRescanTimer.Tick += SessionRescanTick;
            sessionRescanTimer.Start();

            this.Shown += new EventHandler((sender, e) => OpenSettingsForm());
            this.FormClosed += new FormClosedEventHandler((sender, e) => CleanupHooks());
        }

        private void OnForegroundEvent(IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
            int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (eventType != EVENT_SYSTEM_FOREGROUND || hwnd == IntPtr.Zero) return;
            Mixer.OnForegroundChanged(hwnd);
        }

        private async void SessionRescanTick(object sender, EventArgs e)
        {
            var newTarget = await Task.Run(() => Mixer.GetTargetProgram());
            Mixer.target = newTarget;
            Mixer.ApplyMuteForCurrentForeground();
        }

        private void CleanupHooks()
        {
            if (winEventHook != IntPtr.Zero)
            {
                UnhookWinEvent(winEventHook);
                winEventHook = IntPtr.Zero;
            }
            if (sessionRescanTimer != null)
            {
                sessionRescanTimer.Stop();
                sessionRescanTimer.Dispose();
                sessionRescanTimer = null;
            }
        }
    }
}
