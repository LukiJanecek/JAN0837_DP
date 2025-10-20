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
            btnGenerateTemplate = new Button();
            txtBoxParam1 = new TextBox();
            lblParam1 = new Label();
            btnStartTIA = new Button();
            lblParam2 = new Label();
            txtBoxParam2 = new TextBox();
            comboBoxTIAprojects = new ComboBox();
            btnOpenProject = new Button();
            btnAddDB = new Button();
            btnCreateProjectPY = new Button();
            btnStartTIAPY = new Button();
            btnOpenProjectPY = new Button();
            btnAddDBPY = new Button();
            txtBoxTIADLL = new TextBox();
            btnChangeTIADLLPath = new Button();
            chBoxChangeTiaDLLPath = new CheckBox();
            statusStripGenerateTIAtemplate.SuspendLayout();
            SuspendLayout();
            // 
            // statusStripGenerateTIAtemplate
            // 
            statusStripGenerateTIAtemplate.ImageScalingSize = new Size(20, 20);
            statusStripGenerateTIAtemplate.Items.AddRange(new ToolStripItem[] { lblStatus1 });
            statusStripGenerateTIAtemplate.Location = new Point(0, 405);
            statusStripGenerateTIAtemplate.Name = "statusStripGenerateTIAtemplate";
            statusStripGenerateTIAtemplate.Size = new Size(600, 26);
            statusStripGenerateTIAtemplate.TabIndex = 3;
            // 
            // lblStatus1
            // 
            lblStatus1.Name = "lblStatus1";
            lblStatus1.Size = new Size(55, 20);
            lblStatus1.Text = "status1";
            // 
            // btnGenerateTemplate
            // 
            btnGenerateTemplate.Location = new Point(257, 16);
            btnGenerateTemplate.Name = "btnGenerateTemplate";
            btnGenerateTemplate.Size = new Size(94, 63);
            btnGenerateTemplate.TabIndex = 4;
            btnGenerateTemplate.Text = "Create new project";
            btnGenerateTemplate.UseVisualStyleBackColor = true;
            btnGenerateTemplate.Click += btnGenerateTemplate_Click;
            // 
            // txtBoxParam1
            // 
            txtBoxParam1.Location = new Point(29, 39);
            txtBoxParam1.Name = "txtBoxParam1";
            txtBoxParam1.Size = new Size(170, 27);
            txtBoxParam1.TabIndex = 5;
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
            // btnStartTIA
            // 
            btnStartTIA.Location = new Point(257, 85);
            btnStartTIA.Name = "btnStartTIA";
            btnStartTIA.Size = new Size(94, 63);
            btnStartTIA.TabIndex = 7;
            btnStartTIA.Text = "Start \r\nTIA Portal";
            btnStartTIA.UseVisualStyleBackColor = true;
            btnStartTIA.Click += btnStartTIA_Click;
            // 
            // lblParam2
            // 
            lblParam2.AutoSize = true;
            lblParam2.Location = new Point(29, 82);
            lblParam2.Name = "lblParam2";
            lblParam2.Size = new Size(87, 20);
            lblParam2.TabIndex = 9;
            lblParam2.Text = "Parameter2:";
            // 
            // txtBoxParam2
            // 
            txtBoxParam2.Location = new Point(29, 105);
            txtBoxParam2.Name = "txtBoxParam2";
            txtBoxParam2.Size = new Size(170, 27);
            txtBoxParam2.TabIndex = 8;
            // 
            // comboBoxTIAprojects
            // 
            comboBoxTIAprojects.FormattingEnabled = true;
            comboBoxTIAprojects.Location = new Point(29, 154);
            comboBoxTIAprojects.Name = "comboBoxTIAprojects";
            comboBoxTIAprojects.Size = new Size(322, 28);
            comboBoxTIAprojects.TabIndex = 11;
            comboBoxTIAprojects.SelectedIndexChanged += comboBoxTIAprojects_SelectedIndexChanged;
            // 
            // btnOpenProject
            // 
            btnOpenProject.Location = new Point(29, 188);
            btnOpenProject.Name = "btnOpenProject";
            btnOpenProject.Size = new Size(94, 63);
            btnOpenProject.TabIndex = 12;
            btnOpenProject.Text = "Open project";
            btnOpenProject.UseVisualStyleBackColor = true;
            btnOpenProject.Click += btnOpenProject_Click;
            // 
            // btnAddDB
            // 
            btnAddDB.Location = new Point(129, 188);
            btnAddDB.Name = "btnAddDB";
            btnAddDB.Size = new Size(94, 63);
            btnAddDB.TabIndex = 13;
            btnAddDB.Text = "Add DB to this project";
            btnAddDB.UseVisualStyleBackColor = true;
            btnAddDB.Click += btnAddDB_Click;
            // 
            // btnCreateProjectPY
            // 
            btnCreateProjectPY.Location = new Point(464, 50);
            btnCreateProjectPY.Name = "btnCreateProjectPY";
            btnCreateProjectPY.Size = new Size(94, 63);
            btnCreateProjectPY.TabIndex = 14;
            btnCreateProjectPY.Text = "PY Create project";
            btnCreateProjectPY.UseVisualStyleBackColor = true;
            btnCreateProjectPY.Click += btnCreateProjectPY_Click;
            // 
            // btnStartTIAPY
            // 
            btnStartTIAPY.Location = new Point(464, 119);
            btnStartTIAPY.Name = "btnStartTIAPY";
            btnStartTIAPY.Size = new Size(94, 63);
            btnStartTIAPY.TabIndex = 15;
            btnStartTIAPY.Text = "PY Start \r\nTIA Portal";
            btnStartTIAPY.UseVisualStyleBackColor = true;
            btnStartTIAPY.Click += btnStartTIAPY_Click;
            // 
            // btnOpenProjectPY
            // 
            btnOpenProjectPY.Location = new Point(464, 188);
            btnOpenProjectPY.Name = "btnOpenProjectPY";
            btnOpenProjectPY.Size = new Size(94, 63);
            btnOpenProjectPY.TabIndex = 16;
            btnOpenProjectPY.Text = "PY Open project";
            btnOpenProjectPY.UseVisualStyleBackColor = true;
            btnOpenProjectPY.Click += btnOpenProjectPY_Click;
            // 
            // btnAddDBPY
            // 
            btnAddDBPY.Location = new Point(464, 259);
            btnAddDBPY.Name = "btnAddDBPY";
            btnAddDBPY.Size = new Size(94, 63);
            btnAddDBPY.TabIndex = 17;
            btnAddDBPY.Text = "PY Add DB to project";
            btnAddDBPY.UseVisualStyleBackColor = true;
            btnAddDBPY.Click += btnAddDBPY_Click;
            // 
            // txtBoxTIADLL
            // 
            txtBoxTIADLL.Location = new Point(29, 259);
            txtBoxTIADLL.Name = "txtBoxTIADLL";
            txtBoxTIADLL.Size = new Size(329, 27);
            txtBoxTIADLL.TabIndex = 18;
            // 
            // btnChangeTIADLLPath
            // 
            btnChangeTIADLLPath.Location = new Point(364, 259);
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
            chBoxChangeTiaDLLPath.Location = new Point(243, 292);
            chBoxChangeTiaDLLPath.Name = "chBoxChangeTiaDLLPath";
            chBoxChangeTiaDLLPath.Size = new Size(115, 24);
            chBoxChangeTiaDLLPath.TabIndex = 20;
            chBoxChangeTiaDLLPath.Text = "Change path";
            chBoxChangeTiaDLLPath.UseVisualStyleBackColor = true;
            chBoxChangeTiaDLLPath.CheckedChanged += chBoxChangeTiaDLLPath_CheckedChanged;
            // 
            // ucTIAControl
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(chBoxChangeTiaDLLPath);
            Controls.Add(btnChangeTIADLLPath);
            Controls.Add(txtBoxTIADLL);
            Controls.Add(btnAddDBPY);
            Controls.Add(btnOpenProjectPY);
            Controls.Add(btnStartTIAPY);
            Controls.Add(btnCreateProjectPY);
            Controls.Add(btnAddDB);
            Controls.Add(btnOpenProject);
            Controls.Add(comboBoxTIAprojects);
            Controls.Add(lblParam2);
            Controls.Add(txtBoxParam2);
            Controls.Add(btnStartTIA);
            Controls.Add(lblParam1);
            Controls.Add(txtBoxParam1);
            Controls.Add(btnGenerateTemplate);
            Controls.Add(statusStripGenerateTIAtemplate);
            Name = "ucTIAControl";
            Size = new Size(600, 431);
            Load += ucGenerateTIAtemplate_Load;
            statusStripGenerateTIAtemplate.ResumeLayout(false);
            statusStripGenerateTIAtemplate.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private StatusStrip statusStripGenerateTIAtemplate;
        private ToolStripStatusLabel lblStatus1;
        private Button btnGenerateTemplate;
        private TextBox txtBoxParam1;
        private Label lblParam1;
        private Button btnStartTIA;
        private Label lblParam2;
        private TextBox txtBoxParam2;
        private ComboBox comboBoxTIAprojects;
        private Button btnOpenProject;
        private Button btnAddDB;
        private Button btnCreateProjectPY;
        private Button btnStartTIAPY;
        private Button btnOpenProjectPY;
        private Button btnAddDBPY;
        private TextBox txtBoxTIADLL;
        private Button btnChangeTIADLLPath;
        private CheckBox chBoxChangeTiaDLLPath;
    }
}
