namespace JAN0837_DP.Forms
{
    partial class ucCommunicationControl
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
            statusStripCommunicationControl = new StatusStrip();
            status1 = new ToolStripStatusLabel();
            statusStripCommunicationControl.SuspendLayout();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(165, 145);
            button1.Name = "button1";
            button1.Size = new Size(94, 29);
            button1.TabIndex = 0;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(237, 245);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(101, 24);
            checkBox1.TabIndex = 1;
            checkBox1.Text = "checkBox1";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(101, 214);
            label1.Name = "label1";
            label1.Size = new Size(50, 20);
            label1.TabIndex = 2;
            label1.Text = "label1";
            // 
            // statusStripCommunicationControl
            // 
            statusStripCommunicationControl.ImageScalingSize = new Size(20, 20);
            statusStripCommunicationControl.Items.AddRange(new ToolStripItem[] { status1 });
            statusStripCommunicationControl.Location = new Point(0, 372);
            statusStripCommunicationControl.Name = "statusStripCommunicationControl";
            statusStripCommunicationControl.Size = new Size(732, 26);
            statusStripCommunicationControl.TabIndex = 3;
            statusStripCommunicationControl.Text = "statusStrip1";
            // 
            // status1
            // 
            status1.Name = "status1";
            status1.Size = new Size(55, 20);
            status1.Text = "status1";
            // 
            // ucCommunicationControl
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(statusStripCommunicationControl);
            Controls.Add(label1);
            Controls.Add(checkBox1);
            Controls.Add(button1);
            Name = "ucCommunicationControl";
            Size = new Size(732, 398);
            Load += CommunicationControl_Load;
            statusStripCommunicationControl.ResumeLayout(false);
            statusStripCommunicationControl.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private CheckBox checkBox1;
        private Label label1;
        private StatusStrip statusStripCommunicationControl;
        private ToolStripStatusLabel status1;
    }
}
