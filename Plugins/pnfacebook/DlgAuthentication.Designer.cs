﻿namespace pnfacebook
{
    partial class DlgAuthentication
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.wbAuth = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // wbAuth
            // 
            this.wbAuth.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wbAuth.Location = new System.Drawing.Point(0, 0);
            this.wbAuth.MinimumSize = new System.Drawing.Size(20, 20);
            this.wbAuth.Name = "wbAuth";
            this.wbAuth.ScriptErrorsSuppressed = true;
            this.wbAuth.ScrollBarsEnabled = false;
            this.wbAuth.Size = new System.Drawing.Size(652, 386);
            this.wbAuth.TabIndex = 0;
            this.wbAuth.Navigated += new System.Windows.Forms.WebBrowserNavigatedEventHandler(this.wbAuth_Navigated);
            // 
            // DlgAuthentication
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(652, 386);
            this.Controls.Add(this.wbAuth);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DlgAuthentication";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login to Facebook";
            this.Load += new System.EventHandler(this.DlgAuthentication_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser wbAuth;
    }
}