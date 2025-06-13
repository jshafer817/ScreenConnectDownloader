namespace ScreenConnectDownloader
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.txtSupportCode = new System.Windows.Forms.TextBox();
            this.lblPrompt = new System.Windows.Forms.Label();
            this.btnJoinSession = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtSupportCode
            // 
            this.txtSupportCode.Location = new System.Drawing.Point(96, 60);
            this.txtSupportCode.Name = "txtSupportCode";
            this.txtSupportCode.Size = new System.Drawing.Size(100, 20);
            this.txtSupportCode.TabIndex = 0;
            // 
            // lblPrompt
            // 
            this.lblPrompt.AutoSize = true;
            this.lblPrompt.Location = new System.Drawing.Point(104, 40);
            this.lblPrompt.Name = "lblPrompt";
            this.lblPrompt.Size = new System.Drawing.Size(85, 13);
            this.lblPrompt.TabIndex = 1;
            this.lblPrompt.Text = "Enter Join Code:";
            // 
            // btnJoinSession
            // 
            this.btnJoinSession.Location = new System.Drawing.Point(96, 90);
            this.btnJoinSession.Name = "btnJoinSession";
            this.btnJoinSession.Size = new System.Drawing.Size(100, 23);
            this.btnJoinSession.TabIndex = 2;
            this.btnJoinSession.Text = "Join Session";
            this.btnJoinSession.UseVisualStyleBackColor = true;
            this.btnJoinSession.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // Form1
            // 
            this.AcceptButton = this.btnJoinSession;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(294, 160);
            this.Controls.Add(this.btnJoinSession);
            this.Controls.Add(this.lblPrompt);
            this.Controls.Add(this.txtSupportCode);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ScreenConnect Downloader";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtSupportCode;
        private System.Windows.Forms.Label lblPrompt;
        private System.Windows.Forms.Button btnJoinSession;
    }
}
