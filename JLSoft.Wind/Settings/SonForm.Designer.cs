namespace JLSoft.Wind.Settings
{
    partial class SonForm
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
            label1 = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("仿宋", 16F, FontStyle.Bold);
            label1.Location = new Point(291, 226);
            label1.Name = "label1";
            label1.Size = new Size(171, 22);
            label1.TabIndex = 0;
            label1.Text = "这是一个子页面";
            // 
            // SonForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(label1);
            Name = "SonForm";
            Text = "SonForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        public Label label1;
    }
}