namespace JAN0837_DP.Forms
{
    partial class ucGenerateTIAtemplate
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
            txtParam1 = new TextBox();
            lblParameter1 = new Label();
            btnStartTIA = new Button();
            statusStripGenerateTIAtemplate.SuspendLayout();
            SuspendLayout();
            // 
            // statusStripGenerateTIAtemplate
            // 
            statusStripGenerateTIAtemplate.ImageScalingSize = new Size(20, 20);
            statusStripGenerateTIAtemplate.Items.AddRange(new ToolStripItem[] { lblStatus1 });
            statusStripGenerateTIAtemplate.Location = new Point(0, 389);
            statusStripGenerateTIAtemplate.Name = "statusStripGenerateTIAtemplate";
            statusStripGenerateTIAtemplate.Size = new Size(748, 26);
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
            btnGenerateTemplate.Location = new Point(404, 39);
            btnGenerateTemplate.Name = "btnGenerateTemplate";
            btnGenerateTemplate.Size = new Size(94, 63);
            btnGenerateTemplate.TabIndex = 4;
            btnGenerateTemplate.Text = "Generate template";
            btnGenerateTemplate.UseVisualStyleBackColor = true;
            btnGenerateTemplate.Click += btnGenerateTemplate_Click;
            // 
            // txtParam1
            // 
            txtParam1.Location = new Point(29, 39);
            txtParam1.Name = "txtParam1";
            txtParam1.Size = new Size(170, 27);
            txtParam1.TabIndex = 5;
            // 
            // lblParameter1
            // 
            lblParameter1.AutoSize = true;
            lblParameter1.Location = new Point(29, 16);
            lblParameter1.Name = "lblParameter1";
            lblParameter1.Size = new Size(87, 20);
            lblParameter1.TabIndex = 6;
            lblParameter1.Text = "Parameter1:";
            // 
            // btnStartTIA
            // 
            btnStartTIA.Location = new Point(404, 108);
            btnStartTIA.Name = "btnStartTIA";
            btnStartTIA.Size = new Size(94, 63);
            btnStartTIA.TabIndex = 7;
            btnStartTIA.Text = "Start TIA";
            btnStartTIA.UseVisualStyleBackColor = true;
            btnStartTIA.Click += btnStartTIA_Click;
            // 
            // ucGenerateTIAtemplate
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(btnStartTIA);
            Controls.Add(lblParameter1);
            Controls.Add(txtParam1);
            Controls.Add(btnGenerateTemplate);
            Controls.Add(statusStripGenerateTIAtemplate);
            Name = "ucGenerateTIAtemplate";
            Size = new Size(748, 415);
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
        private TextBox txtParam1;
        private Label lblParameter1;
        private Button btnStartTIA;
    }
}
