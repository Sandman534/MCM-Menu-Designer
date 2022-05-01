namespace MCMHelper_MenuDesigner
{
    partial class JSONWindow
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
            this.txtJSONOutput = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // txtJSONOutput
            // 
            this.txtJSONOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtJSONOutput.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtJSONOutput.Location = new System.Drawing.Point(0, 0);
            this.txtJSONOutput.Name = "txtJSONOutput";
            this.txtJSONOutput.Size = new System.Drawing.Size(786, 760);
            this.txtJSONOutput.TabIndex = 0;
            this.txtJSONOutput.Text = "";
            // 
            // JSONWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(786, 760);
            this.Controls.Add(this.txtJSONOutput);
            this.Name = "JSONWindow";
            this.Text = "Text Output";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox txtJSONOutput;
    }
}