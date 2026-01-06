namespace JAN0837_DP.Forms
{
    partial class ucTIAControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            statusStripGenerateTIAtemplate = new StatusStrip();
            lblStatus1 = new ToolStripStatusLabel();
            txtBoxParam1 = new TextBox();
            lblParam1 = new Label();
            lblParam2 = new Label();
            txtBoxParam2 = new TextBox();
            comboBoxTIAprojects = new ComboBox();
            btnCreateProjectPY = new Button();
            btnStartTIAPY = new Button();
            btnOpenProjectPY = new Button();
            btnAddDBPY = new Button();
            txtBoxTIADLL = new TextBox();
            btnChangeTIADLLPath = new Button();
            chBoxChangeTiaDLLPath = new CheckBox();
            lblTiaProject = new Label();
            lblDLLpath = new Label();
            btnImportDLL = new Button();
            btnPreset = new Button();
            lblParam3 = new Label();
            txtBoxParam3 = new TextBox();
            lblParam4 = new Label();
            txtBoxParam4 = new TextBox();
            statusStripGenerateTIAtemplate.SuspendLayout();
            SuspendLayout();
            // 
            // statusStripGenerateTIAtemplate
            // 
            statusStripGenerateTIAtemplate.ImageScalingSize = new Size(20, 20);
            statusStripGenerateTIAtemplate.Items.AddRange(new ToolStripItem[] { lblStatus1 });
            statusStripGenerateTIAtemplate.Location = new Point(0, 460);
            statusStripGenerateTIAtemplate.Name = "statusStripGenerateTIAtemplate";
            statusStripGenerateTIAtemplate.Size = new Size(773, 26);
            statusStripGenerateTIAtemplate.TabIndex = 3;
            // 
            // lblStatus1
            // 
            lblStatus1.Name = "lblStatus1";
            lblStatus1.Size = new Size(55, 20);
            lblStatus1.Text = "status1";
            // 
            // txtBoxParam1
            // 
            txtBoxParam1.Location = new Point(29, 39);
            txtBoxParam1.Name = "txtBoxParam1";
            txtBoxParam1.Size = new Size(325, 27);
            txtBoxParam1.TabIndex = 5;
            txtBoxParam1.TextChanged += txtBoxParam1_TextChanged;
            // 
            // lblParam1
            // 
            lblParam1.AutoSize = true;
            lblParam1.Location = new Point(29, 16);
            lblParam1.Name = "lblParam1";
            lblParam1.Size = new Size(87, 20);
            lblParam1.TabIndex = 6;
            lblParam1.Text = "Parameter1:";
            // 
            // lblParam2
            // 
            lblParam2.AutoSize = true;
            lblParam2.Location = new Point(29, 69);
            lblParam2.Name = "lblParam2";
            lblParam2.Size = new Size(87, 20);
            lblParam2.TabIndex = 9;
            lblParam2.Text = "Parameter2:";
            // 
            // txtBoxParam2
            // 
            txtBoxParam2.Location = new Point(29, 92);
            txtBoxParam2.Name = "txtBoxParam2";
            txtBoxParam2.Size = new Size(325, 27);
            txtBoxParam2.TabIndex = 8;
            txtBoxParam2.TextChanged += txtBoxParam2_TextChanged;
            // 
            // comboBoxTIAprojects
            // 
            comboBoxTIAprojects.FormattingEnabled = true;
            comboBoxTIAprojects.Location = new Point(32, 268);
            comboBoxTIAprojects.Name = "comboBoxTIAprojects";
            comboBoxTIAprojects.Size = new Size(322, 28);
            comboBoxTIAprojects.TabIndex = 11;
            comboBoxTIAprojects.SelectedIndexChanged += comboBoxTIAprojects_SelectedIndexChanged;
            // 
            // btnCreateProjectPY
            // 
            btnCreateProjectPY.Location = new Point(360, 21);
            btnCreateProjectPY.Name = "btnCreateProjectPY";
            btnCreateProjectPY.Size = new Size(94, 63);
            btnCreateProjectPY.TabIndex = 14;
            btnCreateProjectPY.Text = "Create project";
            btnCreateProjectPY.UseVisualStyleBackColor = true;
            btnCreateProjectPY.Click += btnCreateProjectPY_Click;
            // 
            // btnStartTIAPY
            // 
            btnStartTIAPY.Location = new Point(460, 21);
            btnStartTIAPY.Name = "btnStartTIAPY";
            btnStartTIAPY.Size = new Size(94, 63);
            btnStartTIAPY.TabIndex = 15;
            btnStartTIAPY.Text = "Start \r\nTIA Portal";
            btnStartTIAPY.UseVisualStyleBackColor = true;
            btnStartTIAPY.Click += btnStartTIAPY_Click;
            // 
            // btnOpenProjectPY
            // 
            btnOpenProjectPY.Location = new Point(360, 250);
            btnOpenProjectPY.Name = "btnOpenProjectPY";
            btnOpenProjectPY.Size = new Size(94, 63);
            btnOpenProjectPY.TabIndex = 16;
            btnOpenProjectPY.Text = "Open project";
            btnOpenProjectPY.UseVisualStyleBackColor = true;
            btnOpenProjectPY.Click += btnOpenProjectPY_Click;
            // 
            // btnAddDBPY
            // 
            btnAddDBPY.Location = new Point(460, 250);
            btnAddDBPY.Name = "btnAddDBPY";
            btnAddDBPY.Size = new Size(94, 63);
            btnAddDBPY.TabIndex = 17;
            btnAddDBPY.Text = "Add DB to project";
            btnAddDBPY.UseVisualStyleBackColor = true;
            btnAddDBPY.Click += btnAddDBPY_Click;
            // 
            // txtBoxTIADLL
            // 
            txtBoxTIADLL.Location = new Point(32, 341);
            txtBoxTIADLL.Name = "txtBoxTIADLL";
            txtBoxTIADLL.Size = new Size(522, 27);
            txtBoxTIADLL.TabIndex = 18;
            txtBoxTIADLL.TextChanged += txtBoxTIADLL_TextChanged;
            // 
            // btnChangeTIADLLPath
            // 
            btnChangeTIADLLPath.Location = new Point(460, 374);
            btnChangeTIADLLPath.Name = "btnChangeTIADLLPath";
            btnChangeTIADLLPath.Size = new Size(94, 63);
            btnChangeTIADLLPath.TabIndex = 19;
            btnChangeTIADLLPath.Text = "Change path";
            btnChangeTIADLLPath.UseVisualStyleBackColor = true;
            btnChangeTIADLLPath.Click += btnChangeTIADLLPath_Click;
            // 
            // chBoxChangeTiaDLLPath
            // 
            chBoxChangeTiaDLLPath.AutoSize = true;
            chBoxChangeTiaDLLPath.Location = new Point(339, 374);
            chBoxChangeTiaDLLPath.Name = "chBoxChangeTiaDLLPath";
            chBoxChangeTiaDLLPath.Size = new Size(115, 24);
            chBoxChangeTiaDLLPath.TabIndex = 20;
            chBoxChangeTiaDLLPath.Text = "Change path";
            chBoxChangeTiaDLLPath.UseVisualStyleBackColor = true;
            chBoxChangeTiaDLLPath.CheckedChanged += chBoxChangeTiaDLLPath_CheckedChanged;
            // 
            // lblTiaProject
            // 
            lblTiaProject.AutoSize = true;
            lblTiaProject.Location = new Point(32, 245);
            lblTiaProject.Name = "lblTiaProject";
            lblTiaProject.Size = new Size(91, 20);
            lblTiaProject.TabIndex = 21;
            lblTiaProject.Text = "TIA projects:";
            // 
            // lblDLLpath
            // 
            lblDLLpath.AutoSize = true;
            lblDLLpath.Location = new Point(32, 318);
            lblDLLpath.Name = "lblDLLpath";
            lblDLLpath.Size = new Size(138, 20);
            lblDLLpath.TabIndex = 22;
            lblDLLpath.Text = "Path to DLL project:";
            // 
            // btnImportDLL
            // 
            btnImportDLL.Location = new Point(32, 374);
            btnImportDLL.Name = "btnImportDLL";
            btnImportDLL.Size = new Size(94, 63);
            btnImportDLL.TabIndex = 23;
            btnImportDLL.Text = "Import DLL";
            btnImportDLL.UseVisualStyleBackColor = true;
            // 
            // btnPreset
            // 
            btnPreset.Location = new Point(360, 90);
            btnPreset.Name = "btnPreset";
            btnPreset.Size = new Size(94, 63);
            btnPreset.TabIndex = 24;
            btnPreset.Text = "Use preset";
            btnPreset.UseVisualStyleBackColor = true;
            btnPreset.Click += btnPreset_Click;
            // 
            // lblParam3
            // 
            lblParam3.AutoSize = true;
            lblParam3.Location = new Point(29, 122);
            lblParam3.Name = "lblParam3";
            lblParam3.Size = new Size(87, 20);
            lblParam3.TabIndex = 25;
            lblParam3.Text = "Parameter3:";
            // 
            // txtBoxParam3
            // 
            txtBoxParam3.Location = new Point(29, 145);
            txtBoxParam3.Name = "txtBoxParam3";
            txtBoxParam3.Size = new Size(325, 27);
            txtBoxParam3.TabIndex = 26;
            // 
            // lblParam4
            // 
            lblParam4.AutoSize = true;
            lblParam4.Location = new Point(29, 175);
            lblParam4.Name = "lblParam4";
            lblParam4.Size = new Size(87, 20);
            lblParam4.TabIndex = 27;
            lblParam4.Text = "Parameter4:";
            // 
            // txtBoxParam4
            // 
            txtBoxParam4.Location = new Point(29, 197);
            txtBoxParam4.Name = "txtBoxParam4";
            txtBoxParam4.Size = new Size(325, 27);
            txtBoxParam4.TabIndex = 28;
            // 
            // ucTIAControl
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(txtBoxParam4);
            Controls.Add(lblParam4);
            Controls.Add(txtBoxParam3);
            Controls.Add(lblParam3);
            Controls.Add(btnPreset);
            Controls.Add(btnImportDLL);
            Controls.Add(lblDLLpath);
            Controls.Add(lblTiaProject);
            Controls.Add(chBoxChangeTiaDLLPath);
            Controls.Add(btnChangeTIADLLPath);
            Controls.Add(txtBoxTIADLL);
            Controls.Add(btnAddDBPY);
            Controls.Add(btnOpenProjectPY);
            Controls.Add(btnStartTIAPY);
            Controls.Add(btnCreateProjectPY);
            Controls.Add(comboBoxTIAprojects);
            Controls.Add(lblParam2);
            Controls.Add(txtBoxParam2);
            Controls.Add(lblParam1);
            Controls.Add(txtBoxParam1);
            Controls.Add(statusStripGenerateTIAtemplate);
            Name = "ucTIAControl";
            Size = new Size(773, 486);
            Load += ucGenerateTIAtemplate_Load;
            statusStripGenerateTIAtemplate.ResumeLayout(false);
            statusStripGenerateTIAtemplate.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private StatusStrip statusStripGenerateTIAtemplate;
        private ToolStripStatusLabel lblStatus1;
        private TextBox txtBoxParam1;
        private Label lblParam1;
        private Label lblParam2;
        private TextBox txtBoxParam2;
        private ComboBox comboBoxTIAprojects;
        private Button btnCreateProjectPY;
        private Button btnStartTIAPY;
        private Button btnOpenProjectPY;
        private Button btnAddDBPY;
        private TextBox txtBoxTIADLL;
        private Button btnChangeTIADLLPath;
        private CheckBox chBoxChangeTiaDLLPath;
        private Label lblTiaProject;
        private Label lblDLLpath;
        private Button btnImportDLL;
        private Button btnPreset;
        private Label lblParam3;
        private TextBox txtBoxParam3;
        private Label lblParam4;
        private TextBox txtBoxParam4;
    }
}
