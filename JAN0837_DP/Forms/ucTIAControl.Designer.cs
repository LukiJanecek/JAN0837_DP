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
            comboBoxTIAprojects = new ComboBox();
            btnCreateProject = new Button();
            btnStartTIA = new Button();
            btnOpenProject = new Button();
            btnAddDB = new Button();
            txtBoxTIADLL = new TextBox();
            chBoxChangeTiaDLLPath = new CheckBox();
            lblTiaProject = new Label();
            lblDLLpath = new Label();
            btnImportDLL = new Button();
            btnPreset = new Button();
            lblParam2 = new Label();
            txtBoxParam2 = new TextBox();
            lblParam3 = new Label();
            txtBoxParam3 = new TextBox();
            rbtnCreateNewProject = new RadioButton();
            rbtnOpenProject = new RadioButton();
            btnFindTIAProjectOnPath = new Button();
            btnChangeDLLPath = new Button();
            btnUsePresetDLLPath = new Button();
            btnChangeProjectPath = new Button();
            statusStripGenerateTIAtemplate.SuspendLayout();
            SuspendLayout();
            // 
            // statusStripGenerateTIAtemplate
            // 
            statusStripGenerateTIAtemplate.ImageScalingSize = new Size(20, 20);
            statusStripGenerateTIAtemplate.Items.AddRange(new ToolStripItem[] { lblStatus1 });
            statusStripGenerateTIAtemplate.Location = new Point(0, 702);
            statusStripGenerateTIAtemplate.Name = "statusStripGenerateTIAtemplate";
            statusStripGenerateTIAtemplate.Size = new Size(1400, 26);
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
            txtBoxParam1.Location = new Point(128, 243);
            txtBoxParam1.Name = "txtBoxParam1";
            txtBoxParam1.Size = new Size(450, 27);
            txtBoxParam1.TabIndex = 5;
            txtBoxParam1.TextChanged += txtBoxParam1_TextChanged;
            // 
            // lblParam1
            // 
            lblParam1.AutoSize = true;
            lblParam1.Location = new Point(128, 220);
            lblParam1.Name = "lblParam1";
            lblParam1.Size = new Size(87, 20);
            lblParam1.TabIndex = 6;
            lblParam1.Text = "Parameter1:";
            // 
            // comboBoxTIAprojects
            // 
            comboBoxTIAprojects.FormattingEnabled = true;
            comboBoxTIAprojects.Location = new Point(128, 365);
            comboBoxTIAprojects.Name = "comboBoxTIAprojects";
            comboBoxTIAprojects.Size = new Size(450, 28);
            comboBoxTIAprojects.TabIndex = 11;
            comboBoxTIAprojects.SelectedIndexChanged += comboBoxTIAprojects_SelectedIndexChanged;
            // 
            // btnCreateProject
            // 
            btnCreateProject.Location = new Point(128, 506);
            btnCreateProject.Name = "btnCreateProject";
            btnCreateProject.Size = new Size(94, 63);
            btnCreateProject.TabIndex = 14;
            btnCreateProject.Text = "Create new project";
            btnCreateProject.UseVisualStyleBackColor = true;
            btnCreateProject.Click += btnCreateProject_Click;
            // 
            // btnStartTIA
            // 
            btnStartTIA.Location = new Point(28, 22);
            btnStartTIA.Name = "btnStartTIA";
            btnStartTIA.Size = new Size(94, 63);
            btnStartTIA.TabIndex = 15;
            btnStartTIA.Text = "Start \r\nTIA Portal";
            btnStartTIA.UseVisualStyleBackColor = true;
            btnStartTIA.Click += btnStartTIA_Click;
            // 
            // btnOpenProject
            // 
            btnOpenProject.Location = new Point(128, 398);
            btnOpenProject.Name = "btnOpenProject";
            btnOpenProject.Size = new Size(94, 63);
            btnOpenProject.TabIndex = 16;
            btnOpenProject.Text = "Open project";
            btnOpenProject.UseVisualStyleBackColor = true;
            btnOpenProject.Click += btnOpenProject_Click;
            // 
            // btnAddDB
            // 
            btnAddDB.Location = new Point(228, 398);
            btnAddDB.Name = "btnAddDB";
            btnAddDB.Size = new Size(94, 63);
            btnAddDB.TabIndex = 17;
            btnAddDB.Text = "Add DB to this project";
            btnAddDB.UseVisualStyleBackColor = true;
            btnAddDB.Click += btnAddDB_Click;
            // 
            // txtBoxTIADLL
            // 
            txtBoxTIADLL.Location = new Point(156, 58);
            txtBoxTIADLL.Name = "txtBoxTIADLL";
            txtBoxTIADLL.Size = new Size(522, 27);
            txtBoxTIADLL.TabIndex = 18;
            txtBoxTIADLL.TextChanged += txtBoxTIADLL_TextChanged;
            // 
            // chBoxChangeTiaDLLPath
            // 
            chBoxChangeTiaDLLPath.AutoSize = true;
            chBoxChangeTiaDLLPath.Location = new Point(463, 91);
            chBoxChangeTiaDLLPath.Name = "chBoxChangeTiaDLLPath";
            chBoxChangeTiaDLLPath.Size = new Size(110, 24);
            chBoxChangeTiaDLLPath.TabIndex = 20;
            chBoxChangeTiaDLLPath.Text = "Change DLL";
            chBoxChangeTiaDLLPath.UseVisualStyleBackColor = true;
            chBoxChangeTiaDLLPath.CheckedChanged += chBoxChangeTiaDLLPath_CheckedChanged;
            // 
            // lblTiaProject
            // 
            lblTiaProject.AutoSize = true;
            lblTiaProject.Location = new Point(128, 342);
            lblTiaProject.Name = "lblTiaProject";
            lblTiaProject.Size = new Size(153, 20);
            lblTiaProject.TabIndex = 21;
            lblTiaProject.Text = "Founded TIA projects:";
            // 
            // lblDLLpath
            // 
            lblDLLpath.AutoSize = true;
            lblDLLpath.Location = new Point(156, 35);
            lblDLLpath.Name = "lblDLLpath";
            lblDLLpath.Size = new Size(138, 20);
            lblDLLpath.TabIndex = 22;
            lblDLLpath.Text = "Path to DLL project:";
            // 
            // btnImportDLL
            // 
            btnImportDLL.Location = new Point(584, 91);
            btnImportDLL.Name = "btnImportDLL";
            btnImportDLL.Size = new Size(94, 63);
            btnImportDLL.TabIndex = 23;
            btnImportDLL.Text = "Try import DLL";
            btnImportDLL.UseVisualStyleBackColor = true;
            btnImportDLL.Click += btnImportDLL_Click;
            // 
            // btnPreset
            // 
            btnPreset.Location = new Point(28, 220);
            btnPreset.Name = "btnPreset";
            btnPreset.Size = new Size(94, 63);
            btnPreset.TabIndex = 24;
            btnPreset.Text = "Use preset";
            btnPreset.UseVisualStyleBackColor = true;
            btnPreset.Click += btnPreset_Click;
            // 
            // lblParam2
            // 
            lblParam2.AutoSize = true;
            lblParam2.Location = new Point(128, 398);
            lblParam2.Name = "lblParam2";
            lblParam2.Size = new Size(87, 20);
            lblParam2.TabIndex = 25;
            lblParam2.Text = "Parameter2:";
            // 
            // txtBoxParam2
            // 
            txtBoxParam2.Location = new Point(128, 421);
            txtBoxParam2.Name = "txtBoxParam2";
            txtBoxParam2.Size = new Size(450, 27);
            txtBoxParam2.TabIndex = 26;
            txtBoxParam2.TextChanged += txtBoxParam3_TextChanged;
            // 
            // lblParam3
            // 
            lblParam3.AutoSize = true;
            lblParam3.Location = new Point(128, 451);
            lblParam3.Name = "lblParam3";
            lblParam3.Size = new Size(87, 20);
            lblParam3.TabIndex = 27;
            lblParam3.Text = "Parameter3:";
            // 
            // txtBoxParam3
            // 
            txtBoxParam3.Location = new Point(128, 473);
            txtBoxParam3.Name = "txtBoxParam3";
            txtBoxParam3.Size = new Size(450, 27);
            txtBoxParam3.TabIndex = 28;
            txtBoxParam3.TextChanged += txtBoxParam3_TextChanged;
            // 
            // rbtnCreateNewProject
            // 
            rbtnCreateNewProject.AutoSize = true;
            rbtnCreateNewProject.Location = new Point(28, 190);
            rbtnCreateNewProject.Name = "rbtnCreateNewProject";
            rbtnCreateNewProject.Size = new Size(219, 24);
            rbtnCreateNewProject.TabIndex = 29;
            rbtnCreateNewProject.TabStop = true;
            rbtnCreateNewProject.Text = "Create new template project";
            rbtnCreateNewProject.UseVisualStyleBackColor = true;
            rbtnCreateNewProject.CheckedChanged += rbtnCreateNewProject_CheckedChanged;
            // 
            // rbtnOpenProject
            // 
            rbtnOpenProject.AutoSize = true;
            rbtnOpenProject.Location = new Point(28, 160);
            rbtnOpenProject.Name = "rbtnOpenProject";
            rbtnOpenProject.Size = new Size(171, 24);
            rbtnOpenProject.TabIndex = 30;
            rbtnOpenProject.TabStop = true;
            rbtnOpenProject.Text = "Open created project";
            rbtnOpenProject.UseVisualStyleBackColor = true;
            rbtnOpenProject.CheckedChanged += rbtnOpenProject_CheckedChanged;
            // 
            // btnFindTIAProjectOnPath
            // 
            btnFindTIAProjectOnPath.Location = new Point(233, 276);
            btnFindTIAProjectOnPath.Name = "btnFindTIAProjectOnPath";
            btnFindTIAProjectOnPath.Size = new Size(94, 63);
            btnFindTIAProjectOnPath.TabIndex = 31;
            btnFindTIAProjectOnPath.Text = "Find TIA project";
            btnFindTIAProjectOnPath.UseVisualStyleBackColor = true;
            btnFindTIAProjectOnPath.Click += btnFindTIAProjectOnPath_Click;
            // 
            // btnChangeDLLPath
            // 
            btnChangeDLLPath.Location = new Point(684, 22);
            btnChangeDLLPath.Name = "btnChangeDLLPath";
            btnChangeDLLPath.Size = new Size(94, 63);
            btnChangeDLLPath.TabIndex = 32;
            btnChangeDLLPath.Text = "Change DLL path";
            btnChangeDLLPath.UseVisualStyleBackColor = true;
            btnChangeDLLPath.Click += btnChangeDLLPath_Click;
            // 
            // btnUsePresetDLLPath
            // 
            btnUsePresetDLLPath.Location = new Point(684, 91);
            btnUsePresetDLLPath.Name = "btnUsePresetDLLPath";
            btnUsePresetDLLPath.Size = new Size(94, 63);
            btnUsePresetDLLPath.TabIndex = 33;
            btnUsePresetDLLPath.Text = "Use preset DLL path";
            btnUsePresetDLLPath.UseVisualStyleBackColor = true;
            btnUsePresetDLLPath.Click += btnUsePresetDLLPath_Click;
            // 
            // btnChangeProjectPath
            // 
            btnChangeProjectPath.Location = new Point(128, 276);
            btnChangeProjectPath.Name = "btnChangeProjectPath";
            btnChangeProjectPath.Size = new Size(99, 63);
            btnChangeProjectPath.TabIndex = 34;
            btnChangeProjectPath.Text = "Change project path";
            btnChangeProjectPath.UseVisualStyleBackColor = true;
            btnChangeProjectPath.Click += btnChangeProjectPath_Click;
            // 
            // ucTIAControl
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(btnChangeProjectPath);
            Controls.Add(btnUsePresetDLLPath);
            Controls.Add(btnChangeDLLPath);
            Controls.Add(btnFindTIAProjectOnPath);
            Controls.Add(rbtnOpenProject);
            Controls.Add(rbtnCreateNewProject);
            Controls.Add(txtBoxParam3);
            Controls.Add(lblParam3);
            Controls.Add(txtBoxParam2);
            Controls.Add(lblParam2);
            Controls.Add(btnPreset);
            Controls.Add(btnImportDLL);
            Controls.Add(lblDLLpath);
            Controls.Add(lblTiaProject);
            Controls.Add(chBoxChangeTiaDLLPath);
            Controls.Add(txtBoxTIADLL);
            Controls.Add(btnAddDB);
            Controls.Add(btnOpenProject);
            Controls.Add(btnStartTIA);
            Controls.Add(btnCreateProject);
            Controls.Add(comboBoxTIAprojects);
            Controls.Add(lblParam1);
            Controls.Add(txtBoxParam1);
            Controls.Add(statusStripGenerateTIAtemplate);
            Name = "ucTIAControl";
            Size = new Size(1400, 728);
            Load += ucTIAControl_Load;
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
        private Button btnCreateProject;
        private Button btnStartTIA;
        private Button btnOpenProject;
        private Button btnAddDB;
        private TextBox txtBoxTIADLL;
        private CheckBox chBoxChangeTiaDLLPath;
        private Label lblTiaProject;
        private Label lblDLLpath;
        private Button btnImportDLL;
        private Button btnPreset;
        private Label lblParam3;
        private TextBox txtBoxParam3;
        private RadioButton rbtnCreateNewProject;
        private RadioButton rbtnOpenProject;
        private Button btnFindTIAProjectOnPath;
        private Button btnChangeDLLPath;
        private Button btnUsePresetDLLPath;
        private Button btnChangeProjectPath;
    }
}
