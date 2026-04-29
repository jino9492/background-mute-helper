namespace BackgroundMuteHelper
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.CheckedListBox lstApps;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnAddManual;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblHint;

        private void InitializeComponent()
        {
            this.lblHeader = new System.Windows.Forms.Label();
            this.lstApps = new System.Windows.Forms.CheckedListBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnAddManual = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblHint = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.Location = new System.Drawing.Point(14, 15);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(301, 15);
            this.lblHeader.TabIndex = 0;
            this.lblHeader.Text = "음소거할 앱을 체크하고 [저장]을 누르세요.";
            // 
            // lstApps
            // 
            this.lstApps.CheckOnClick = true;
            this.lstApps.FormattingEnabled = true;
            this.lstApps.IntegralHeight = false;
            this.lstApps.Location = new System.Drawing.Point(16, 40);
            this.lstApps.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.lstApps.Name = "lstApps";
            this.lstApps.Size = new System.Drawing.Size(411, 299);
            this.lstApps.TabIndex = 1;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(16, 350);
            this.btnRefresh.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(91, 35);
            this.btnRefresh.TabIndex = 2;
            this.btnRefresh.Text = "새로고침";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnAddManual
            // 
            this.btnAddManual.Location = new System.Drawing.Point(114, 350);
            this.btnAddManual.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnAddManual.Name = "btnAddManual";
            this.btnAddManual.Size = new System.Drawing.Size(91, 35);
            this.btnAddManual.TabIndex = 3;
            this.btnAddManual.Text = "직접 추가";
            this.btnAddManual.UseVisualStyleBackColor = true;
            this.btnAddManual.Click += new System.EventHandler(this.btnAddManual_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.Location = new System.Drawing.Point(213, 350);
            this.btnRemove.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(91, 35);
            this.btnRemove.TabIndex = 4;
            this.btnRemove.Text = "선택 제거";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(224, 425);
            this.btnSave.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(98, 38);
            this.btnSave.TabIndex = 6;
            this.btnSave.Text = "저장";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(329, 425);
            this.btnClose.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(98, 38);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "닫기";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lblHint
            // 
            this.lblHint.AutoSize = true;
            this.lblHint.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblHint.Location = new System.Drawing.Point(14, 398);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(472, 15);
            this.lblHint.TabIndex = 5;
            this.lblHint.Text = "창을 닫으면 트레이로 숨고, 트레이 메뉴의 Exit로 완전히 종료됩니다.";
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(443, 480);
            this.Controls.Add(this.lblHeader);
            this.Controls.Add(this.lstApps);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnAddManual);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.lblHint);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Background Mute Helper (GUI)";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsForm_FormClosing);
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
