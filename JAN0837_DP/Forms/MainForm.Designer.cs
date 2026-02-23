namespace JAN0837_DP
{
    partial class MainForm
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            toolStripMain = new ToolStrip();
            btnCommunicationControl = new ToolStripButton();
            btnGenerateTIATemplate = new ToolStripButton();
            btnLocalHost = new ToolStripButton();
            btnExit = new ToolStripButton();
            mainWindow = new Panel();
            PeriodicalReading = new System.Windows.Forms.Timer(components);
            toolStripMain.SuspendLayout();
            SuspendLayout();
            // 
            // toolStripMain
            // 
            toolStripMain.ImageScalingSize = new Size(20, 20);
            toolStripMain.Items.AddRange(new ToolStripItem[] { btnCommunicationControl, btnGenerateTIATemplate, btnLocalHost, btnExit });
            toolStripMain.Location = new Point(0, 0);
            toolStripMain.Name = "toolStripMain";
            toolStripMain.Size = new Size(832, 27);
            toolStripMain.TabIndex = 1;
            toolStripMain.Text = "toolStrip1";
            // 
            // btnCommunicationControl
            // 
            btnCommunicationControl.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnCommunicationControl.Image = (Image)resources.GetObject("btnCommunicationControl.Image");
            btnCommunicationControl.ImageTransparentColor = Color.Magenta;
            btnCommunicationControl.Name = "btnCommunicationControl";
            btnCommunicationControl.Size = new Size(169, 24);
            btnCommunicationControl.Text = "Communication control";
            btnCommunicationControl.Click += btnCommunicationControl_Click;
            // 
            // btnGenerateTIATemplate
            // 
            btnGenerateTIATemplate.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnGenerateTIATemplate.Image = (Image)resources.GetObject("btnGenerateTIATemplate.Image");
            btnGenerateTIATemplate.ImageTransparentColor = Color.Magenta;
            btnGenerateTIATemplate.Name = "btnGenerateTIATemplate";
            btnGenerateTIATemplate.Size = new Size(128, 24);
            btnGenerateTIATemplate.Text = "TIA Portal control";
            btnGenerateTIATemplate.Click += btnGenerateTIATemplate_Click;
            // 
            // btnLocalHost
            // 
            btnLocalHost.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnLocalHost.Image = (Image)resources.GetObject("btnLocalHost.Image");
            btnLocalHost.ImageTransparentColor = Color.Magenta;
            btnLocalHost.Name = "btnLocalHost";
            btnLocalHost.Size = new Size(110, 24);
            btnLocalHost.Text = "Open frontend";
            btnLocalHost.Click += btnLocalHost_Click;
            // 
            // btnExit
            // 
            btnExit.Alignment = ToolStripItemAlignment.Right;
            btnExit.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnExit.Image = (Image)resources.GetObject("btnExit.Image");
            btnExit.ImageTransparentColor = Color.Magenta;
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(37, 24);
            btnExit.Text = "Exit";
            btnExit.Click += btnExit_Click;
            // 
            // mainWindow
            // 
            mainWindow.BorderStyle = BorderStyle.FixedSingle;
            mainWindow.Dock = DockStyle.Fill;
            mainWindow.Location = new Point(0, 27);
            mainWindow.Name = "mainWindow";
            mainWindow.Size = new Size(832, 356);
            mainWindow.TabIndex = 21;
            mainWindow.Paint += mainWindow_Paint;
            // 
            // PeriodicalReading
            // 
            PeriodicalReading.Tick += PeriodicalReading_Tick;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(832, 383);
            Controls.Add(mainWindow);
            Controls.Add(toolStripMain);
            Name = "MainForm";
            ShowIcon = false;
            Load += MainForm_Load;
            toolStripMain.ResumeLayout(false);
            toolStripMain.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private ToolStrip toolStripMain;
        private ToolStripButton btnGenerateTIATemplate;
        private ToolStripButton btnLocalHost;
        private ToolStripButton btnExit;
        private ToolStripButton btnCommunicationControl;
        private System.Windows.Forms.Timer PeriodicalReading;
        private Panel mainWindow;
    }
}
