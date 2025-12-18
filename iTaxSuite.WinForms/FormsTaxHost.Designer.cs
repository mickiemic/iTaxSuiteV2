namespace iTaxSuite.WinForms
{
    partial class FormsTaxHost
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormsTaxHost));
            menuStrip1 = new MenuStrip();
            eTimsClientToolStripMenuItem = new ToolStripMenuItem();
            eTimsLaunchMenuItem = new ToolStripMenuItem();
            eTimsProcessRequest = new ToolStripMenuItem();
            eTimsCloseMenuItem = new ToolStripMenuItem();
            zFPClientToolStripMenuItem = new ToolStripMenuItem();
            zfpLaunchMenuItem = new ToolStripMenuItem();
            zfpProcessRequest = new ToolStripMenuItem();
            zfpCloseMenuItem = new ToolStripMenuItem();
            tevinClientToolStripMenuItem = new ToolStripMenuItem();
            tevinLaunchMenuItem = new ToolStripMenuItem();
            tevinClearReqMenuItem = new ToolStripMenuItem();
            tevinClearRespMenuItem = new ToolStripMenuItem();
            clearAllToolStripMenuItem = new ToolStripMenuItem();
            tevinProcessRequest = new ToolStripMenuItem();
            tevinCloseMenuItem = new ToolStripMenuItem();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel = new ToolStripStatusLabel();
            menuStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(24, 24);
            menuStrip1.Items.AddRange(new ToolStripItem[] { eTimsClientToolStripMenuItem, zFPClientToolStripMenuItem, tevinClientToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(7, 2, 0, 2);
            menuStrip1.Size = new Size(1817, 38);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // eTimsClientToolStripMenuItem
            // 
            eTimsClientToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { eTimsLaunchMenuItem, eTimsProcessRequest, eTimsCloseMenuItem });
            eTimsClientToolStripMenuItem.Name = "eTimsClientToolStripMenuItem";
            eTimsClientToolStripMenuItem.Size = new Size(152, 34);
            eTimsClientToolStripMenuItem.Text = "E-Tims Client";
            // 
            // eTimsLaunchMenuItem
            // 
            eTimsLaunchMenuItem.Name = "eTimsLaunchMenuItem";
            eTimsLaunchMenuItem.Size = new Size(281, 40);
            eTimsLaunchMenuItem.Text = "Launch";
            eTimsLaunchMenuItem.Click += eTimsLaunchMenuItem_Click;
            // 
            // eTimsProcessRequest
            // 
            eTimsProcessRequest.Enabled = false;
            eTimsProcessRequest.Name = "eTimsProcessRequest";
            eTimsProcessRequest.Size = new Size(281, 40);
            eTimsProcessRequest.Text = "Process Request";
            // 
            // eTimsCloseMenuItem
            // 
            eTimsCloseMenuItem.Enabled = false;
            eTimsCloseMenuItem.Name = "eTimsCloseMenuItem";
            eTimsCloseMenuItem.Size = new Size(281, 40);
            eTimsCloseMenuItem.Text = "Close";
            eTimsCloseMenuItem.Click += eTimsCloseMenuItem_Click;
            // 
            // zFPClientToolStripMenuItem
            // 
            zFPClientToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { zfpLaunchMenuItem, zfpProcessRequest, zfpCloseMenuItem });
            zFPClientToolStripMenuItem.Name = "zFPClientToolStripMenuItem";
            zFPClientToolStripMenuItem.Size = new Size(124, 34);
            zFPClientToolStripMenuItem.Text = "ZFP Client";
            // 
            // zfpLaunchMenuItem
            // 
            zfpLaunchMenuItem.Name = "zfpLaunchMenuItem";
            zfpLaunchMenuItem.Size = new Size(281, 40);
            zfpLaunchMenuItem.Text = "Launch";
            zfpLaunchMenuItem.Click += zfpLaunchMenuItem_Click;
            // 
            // zfpProcessRequest
            // 
            zfpProcessRequest.Enabled = false;
            zfpProcessRequest.Name = "zfpProcessRequest";
            zfpProcessRequest.Size = new Size(281, 40);
            zfpProcessRequest.Text = "Process Request";
            // 
            // zfpCloseMenuItem
            // 
            zfpCloseMenuItem.Enabled = false;
            zfpCloseMenuItem.Name = "zfpCloseMenuItem";
            zfpCloseMenuItem.Size = new Size(281, 40);
            zfpCloseMenuItem.Text = "Close";
            zfpCloseMenuItem.Click += zfpCloseMenuItem_Click;
            // 
            // tevinClientToolStripMenuItem
            // 
            tevinClientToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { tevinLaunchMenuItem, tevinClearReqMenuItem, tevinClearRespMenuItem, clearAllToolStripMenuItem, tevinProcessRequest, tevinCloseMenuItem });
            tevinClientToolStripMenuItem.Name = "tevinClientToolStripMenuItem";
            tevinClientToolStripMenuItem.Size = new Size(137, 34);
            tevinClientToolStripMenuItem.Text = "Tevin Client";
            // 
            // tevinLaunchMenuItem
            // 
            tevinLaunchMenuItem.Name = "tevinLaunchMenuItem";
            tevinLaunchMenuItem.Size = new Size(281, 40);
            tevinLaunchMenuItem.Text = "Launch";
            tevinLaunchMenuItem.Click += tevinLaunchMenuItem_Click;
            // 
            // tevinClearReqMenuItem
            // 
            tevinClearReqMenuItem.Enabled = false;
            tevinClearReqMenuItem.Name = "tevinClearReqMenuItem";
            tevinClearReqMenuItem.Size = new Size(281, 40);
            tevinClearReqMenuItem.Text = "Clear Request";
            // 
            // tevinClearRespMenuItem
            // 
            tevinClearRespMenuItem.Enabled = false;
            tevinClearRespMenuItem.Name = "tevinClearRespMenuItem";
            tevinClearRespMenuItem.Size = new Size(281, 40);
            tevinClearRespMenuItem.Text = "Clear Response";
            // 
            // clearAllToolStripMenuItem
            // 
            clearAllToolStripMenuItem.Enabled = false;
            clearAllToolStripMenuItem.Name = "clearAllToolStripMenuItem";
            clearAllToolStripMenuItem.Size = new Size(281, 40);
            clearAllToolStripMenuItem.Text = "Clear All";
            // 
            // tevinProcessRequest
            // 
            tevinProcessRequest.Enabled = false;
            tevinProcessRequest.Name = "tevinProcessRequest";
            tevinProcessRequest.Size = new Size(281, 40);
            tevinProcessRequest.Text = "Process Request";
            // 
            // tevinCloseMenuItem
            // 
            tevinCloseMenuItem.Enabled = false;
            tevinCloseMenuItem.Name = "tevinCloseMenuItem";
            tevinCloseMenuItem.Size = new Size(281, 40);
            tevinCloseMenuItem.Text = "Close";
            tevinCloseMenuItem.Click += tevinCloseMenuItem_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(28, 28);
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel });
            statusStrip1.Location = new Point(0, 1094);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1817, 39);
            statusStrip1.TabIndex = 3;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel
            // 
            toolStripStatusLabel.Name = "toolStripStatusLabel";
            toolStripStatusLabel.Size = new Size(195, 30);
            toolStripStatusLabel.Text = "toolStripStatusLabel";
            // 
            // DeskTaxClient
            // 
            AutoScaleDimensions = new SizeF(12F, 30F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1817, 1133);
            Controls.Add(statusStrip1);
            Controls.Add(menuStrip1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            IsMdiContainer = true;
            MainMenuStrip = menuStrip1;
            Margin = new Padding(4);
            Name = "DeskTaxClient";
            Text = "Tax Client";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem eTimsClientToolStripMenuItem;
        private ToolStripMenuItem zFPClientToolStripMenuItem;
        private ToolStripMenuItem tevinClientToolStripMenuItem;
        private ToolStripMenuItem tevinLaunchMenuItem;
        private ToolStripMenuItem tevinCloseMenuItem;
        private ToolStripMenuItem tevinClearReqMenuItem;
        private ToolStripMenuItem tevinClearRespMenuItem;
        private ToolStripMenuItem clearAllToolStripMenuItem;
        private ToolStripMenuItem tevinProcessRequest;
        private ToolStripMenuItem zfpLaunchMenuItem;
        private ToolStripMenuItem zfpCloseMenuItem;
        private ToolStripMenuItem eTimsLaunchMenuItem;
        private ToolStripMenuItem eTimsCloseMenuItem;
        private ToolStripMenuItem eTimsProcessRequest;
        private ToolStripMenuItem zfpProcessRequest;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel;
    }
}
