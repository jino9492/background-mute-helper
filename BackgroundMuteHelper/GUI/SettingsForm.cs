using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace BackgroundMuteHelper
{
    // CheckedListBox toggles the focused item when CheckOnClick is true and
    // the user clicks the empty area below the items. Suppress those clicks.
    internal class ItemOnlyCheckedListBox : CheckedListBox
    {
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONDBLCLK = 0x0203;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_LBUTTONDOWN || m.Msg == WM_LBUTTONDBLCLK)
            {
                int lParam = m.LParam.ToInt32();
                int x = (short)(lParam & 0xFFFF);
                int y = (short)((lParam >> 16) & 0xFFFF);
                int index = IndexFromPoint(x, y);
                if (index < 0 || index >= Items.Count)
                {
                    return;
                }
            }
            base.WndProc(ref m);
        }
    }

    public partial class SettingsForm : Form
    {
        private bool allowExit = false;

        public SettingsForm()
        {
            InitializeComponent();
            LoadIconFromSettings();
            LoadAutoOpenGui();
            ReloadList();
        }

        private void LoadAutoOpenGui()
        {
            chkAutoOpen.CheckedChanged -= chkAutoOpen_CheckedChanged;
            chkAutoOpen.Checked = AppPreferences.GetAutoOpenGui();
            chkAutoOpen.CheckedChanged += chkAutoOpen_CheckedChanged;
        }

        private void chkAutoOpen_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                AppPreferences.SetAutoOpenGui(chkAutoOpen.Checked);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "설정 저장 중 오류가 발생했습니다: " + ex.Message,
                    "Background Mute Helper", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadIconFromSettings()
        {
            try
            {
                dynamic jsonObject = JsonConvert.DeserializeObject(EmbeddedAssets.ReadSettingJson());
                string iconName = (string)jsonObject["icon"];
                Icon icon = EmbeddedAssets.LoadIcon(iconName);
                if (icon != null)
                {
                    this.Icon = icon;
                }
            }
            catch
            {
            }
        }

        public void RequestExit()
        {
            allowExit = true;
        }

        private void ReloadList()
        {
            List<string> selected = MuteTargets.GetProgramList();
            List<string> running = AudioSessionInspector.GetRunningAudioProcessNames();

            HashSet<string> union = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string s in selected) union.Add(s);
            foreach (string s in running) union.Add(s);

            List<string> sorted = union.OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToList();
            HashSet<string> selectedSet = new HashSet<string>(selected, StringComparer.OrdinalIgnoreCase);

            lstApps.BeginUpdate();
            lstApps.Items.Clear();
            foreach (string name in sorted)
            {
                lstApps.Items.Add(name, selectedSet.Contains(name));
            }
            lstApps.EndUpdate();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            HashSet<string> currentlyChecked = new HashSet<string>(
                lstApps.CheckedItems.Cast<string>(),
                StringComparer.OrdinalIgnoreCase);

            List<string> running = AudioSessionInspector.GetRunningAudioProcessNames();
            HashSet<string> union = new HashSet<string>(currentlyChecked, StringComparer.OrdinalIgnoreCase);
            foreach (string s in running) union.Add(s);
            foreach (object item in lstApps.Items) union.Add((string)item);

            List<string> sorted = union.OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToList();

            lstApps.BeginUpdate();
            lstApps.Items.Clear();
            foreach (string name in sorted)
            {
                lstApps.Items.Add(name, currentlyChecked.Contains(name));
            }
            lstApps.EndUpdate();
        }

        private void btnAddManual_Click(object sender, EventArgs e)
        {
            string name = PromptForName();
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            name = name.Trim();
            for (int i = 0; i < lstApps.Items.Count; i++)
            {
                if (string.Equals((string)lstApps.Items[i], name, StringComparison.OrdinalIgnoreCase))
                {
                    lstApps.SetItemChecked(i, true);
                    lstApps.SelectedIndex = i;
                    return;
                }
            }

            int idx = lstApps.Items.Add(name, true);
            lstApps.SelectedIndex = idx;
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            int idx = lstApps.SelectedIndex;
            if (idx < 0)
            {
                MessageBox.Show(this, "제거할 항목을 선택하세요.", "Background Mute Helper",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            lstApps.Items.RemoveAt(idx);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            List<string> programs = lstApps.CheckedItems.Cast<string>().ToList();
            try
            {
                MuteTargets.Save(programs);
                Mixer.RefreshTargets();
                MessageBox.Show(this, "저장되었습니다.", "Background Mute Helper",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "저장 중 오류가 발생했습니다: " + ex.Message,
                    "Background Mute Helper", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!allowExit && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        public void ShowFromTray()
        {
            if (!this.Visible)
            {
                this.Show();
            }
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            this.Activate();
            this.BringToFront();
            ReloadList();
        }

        private string PromptForName()
        {
            using (Form prompt = new Form())
            {
                prompt.Width = 360;
                prompt.Height = 150;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.Text = "프로세스 이름 추가";
                prompt.StartPosition = FormStartPosition.CenterParent;
                prompt.MinimizeBox = false;
                prompt.MaximizeBox = false;

                Label lbl = new Label()
                {
                    Left = 12,
                    Top = 12,
                    Width = 320,
                    Text = "프로세스 이름 (확장자 .exe 제외):"
                };
                TextBox txt = new TextBox() { Left = 12, Top = 36, Width = 320 };
                Button ok = new Button()
                {
                    Text = "확인",
                    Left = 168,
                    Width = 75,
                    Top = 70,
                    DialogResult = DialogResult.OK
                };
                Button cancel = new Button()
                {
                    Text = "취소",
                    Left = 252,
                    Width = 75,
                    Top = 70,
                    DialogResult = DialogResult.Cancel
                };

                prompt.Controls.Add(lbl);
                prompt.Controls.Add(txt);
                prompt.Controls.Add(ok);
                prompt.Controls.Add(cancel);
                prompt.AcceptButton = ok;
                prompt.CancelButton = cancel;

                return prompt.ShowDialog(this) == DialogResult.OK ? txt.Text : null;
            }
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {

        }
    }
}
