﻿namespace MattManela.OpenWithTest.OptionPages
{
    partial class ResetOptionsControl
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.deleteSolutionIndexFileButton = new System.Windows.Forms.Button();
            this.resetIndexedDirectoriesLabel = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.deleteSolutionIndexFileButton);
            this.groupBox1.Controls.Add(this.resetIndexedDirectoriesLabel);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(560, 174);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "OpenWithTest Resets";
            // 
            // deleteSolutionIndexFileButton
            // 
            this.deleteSolutionIndexFileButton.Location = new System.Drawing.Point(6, 33);
            this.deleteSolutionIndexFileButton.Name = "deleteSolutionIndexFileButton";
            this.deleteSolutionIndexFileButton.Size = new System.Drawing.Size(149, 23);
            this.deleteSolutionIndexFileButton.TabIndex = 2;
            this.deleteSolutionIndexFileButton.Text = "Delete Solution Index File";
            this.deleteSolutionIndexFileButton.UseVisualStyleBackColor = true;
            this.deleteSolutionIndexFileButton.Click += new System.EventHandler(this.deleteSolutionIndexFile_Click);
            // 
            // resetIndexedDirectoriesLabel
            // 
            this.resetIndexedDirectoriesLabel.AutoEllipsis = true;
            this.resetIndexedDirectoriesLabel.AutoSize = true;
            this.resetIndexedDirectoriesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resetIndexedDirectoriesLabel.Location = new System.Drawing.Point(25, 59);
            this.resetIndexedDirectoriesLabel.Name = "resetIndexedDirectoriesLabel";
            this.resetIndexedDirectoriesLabel.Size = new System.Drawing.Size(379, 54);
            this.resetIndexedDirectoriesLabel.TabIndex = 1;
            this.resetIndexedDirectoriesLabel.Text = "Deletes the current solution\'s index filed.  \r\nThis will also stop the current in" +
                "dexing process if active.  \r\nThe index will be rebuilt next time you open the so" +
                "lution.";
            // 
            // ResetOptionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "ResetOptionsControl";
            this.Size = new System.Drawing.Size(560, 174);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label resetIndexedDirectoriesLabel;
        private System.Windows.Forms.Button deleteSolutionIndexFileButton;
    }
}
