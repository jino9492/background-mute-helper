using System;
using System.Windows.Forms;

namespace BackgroundMuteHelper
{
    public partial class BackgroundMuteHelper
    {
        private SettingsForm settingsForm;

        public void OpenSettingsForm()
        {
            if (settingsForm == null || settingsForm.IsDisposed)
            {
                settingsForm = new SettingsForm();
            }
            settingsForm.ShowFromTray();
        }

        public void ExitApplication()
        {
            try
            {
                if (settingsForm != null && !settingsForm.IsDisposed)
                {
                    settingsForm.RequestExit();
                    settingsForm.Close();
                }
            }
            catch
            {
            }

            try
            {
                if (notifyIcon != null)
                {
                    notifyIcon.Visible = false;
                }
            }
            catch
            {
            }

            Application.Exit();
        }
    }
}
