using System.Windows.Forms;

namespace LinesGame
{
    partial class LinesForm : Form
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
            this.components = new System.ComponentModel.Container();
            this.pbCenter = new System.Windows.Forms.PictureBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize) (this.pbCenter)).BeginInit();
            this.SuspendLayout();
            // 
            // pbCenter
            // 
            this.pbCenter.BackColor = System.Drawing.Color.Linen;
            this.pbCenter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbCenter.Location = new System.Drawing.Point(0, 0);
            this.pbCenter.Name = "pbCenter";
            this.pbCenter.Size = new System.Drawing.Size(700, 700);
            this.pbCenter.TabIndex = 0;
            this.pbCenter.TabStop = false;
            this.pbCenter.Paint += new System.Windows.Forms.PaintEventHandler(this.pbCenter_Paint);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // LinesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(700, 700);
            this.Controls.Add(this.pbCenter);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "LinesForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "BridgeIt";
            this.Load += new System.EventHandler(this.LinesForm_Load);
            ((System.ComponentModel.ISupportInitialize) (this.pbCenter)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.PictureBox pbCenter;
        private System.Windows.Forms.Timer timer1;
    }
}