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
            button1 = new Button();
            checkBox1 = new CheckBox();
            label1 = new Label();
            statusStripGenerateTIAtemplate = new StatusStrip();
            status1 = new ToolStripStatusLabel();
            statusStripGenerateTIAtemplate.SuspendLayout();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(60, 42);
            button1.Name = "button1";
            button1.Size = new Size(94, 29);
            button1.TabIndex = 0;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(226, 99);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(101, 24);
            checkBox1.TabIndex = 1;
            checkBox1.Text = "checkBox1";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(124, 202);
            label1.Name = "label1";
            label1.Size = new Size(50, 20);
            label1.TabIndex = 2;
            label1.Text = "label1";
            // 
            // statusStripGenerateTIAtemplate
            // 
            statusStripGenerateTIAtemplate.ImageScalingSize = new Size(20, 20);
            statusStripGenerateTIAtemplate.Items.AddRange(new ToolStripItem[] { status1 });
            statusStripGenerateTIAtemplate.Location = new Point(0, 389);
            statusStripGenerateTIAtemplate.Name = "statusStripGenerateTIAtemplate";
            statusStripGenerateTIAtemplate.Size = new Size(748, 26);
            statusStripGenerateTIAtemplate.TabIndex = 3;
            // 
            // status1
            // 
            status1.Name = "status1";
            status1.Size = new Size(55, 20);
            status1.Text = "status1";
            // 
            // ucGenerateTIAtemplate
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(statusStripGenerateTIAtemplate);
            Controls.Add(label1);
            Controls.Add(checkBox1);
            Controls.Add(button1);
            Name = "ucGenerateTIAtemplate";
            Size = new Size(748, 415);
            Load += ucGenerateTIAtemplate_Load;
            statusStripGenerateTIAtemplate.ResumeLayout(false);
            statusStripGenerateTIAtemplate.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private CheckBox checkBox1;
        private Label label1;
        private StatusStrip statusStripGenerateTIAtemplate;
        private ToolStripStatusLabel status1;
    }
}
