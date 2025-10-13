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
            statusStripGenerateTIAtemplate.SuspendLayout();
            SuspendLayout();
            // 
            // statusStripGenerateTIAtemplate
            // 
            statusStripGenerateTIAtemplate.ImageScalingSize = new Size(20, 20);
            statusStripGenerateTIAtemplate.Items.AddRange(new ToolStripItem[] { lblStatus1 });
            statusStripGenerateTIAtemplate.Location = new Point(0, 405);
            statusStripGenerateTIAtemplate.Name = "statusStripGenerateTIAtemplate";
            statusStripGenerateTIAtemplate.Size = new Size(372, 26);
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
            btnOpenProject.Location = new Point(29, 242);
            btnOpenProject.Name = "btnOpenProject";
            btnOpenProject.Size = new Size(94, 63);
            btnOpenProject.TabIndex = 12;
            btnOpenProject.Text = "Open project";
            btnOpenProject.UseVisualStyleBackColor = true;
            btnOpenProject.Click += btnOpenProject_Click;
            // 
            // btnAddDB
            // 
            btnAddDB.Location = new Point(129, 242);
            btnAddDB.Name = "btnAddDB";
            btnAddDB.Size = new Size(94, 63);
            btnAddDB.TabIndex = 13;
            btnAddDB.Text = "Add DB to this project";
            btnAddDB.UseVisualStyleBackColor = true;
            btnAddDB.Click += btnAddDB_Click;
            // 
            // ucTIAControl
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
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
            Size = new Size(372, 431);
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
    }
}
