using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace LinesGame
{
    public partial class LinesForm : Form
    {
        private LinesGame game;
        
        public LinesForm()
        {
            InitializeComponent();
            this.Size = Config.WINDOW_SIZE;
            if (pbCenter.Width <= pbCenter.Height)
                this.Width += (pbCenter.Height - pbCenter.Width);
            else
                this.Height += (pbCenter.Width - pbCenter.Height);
            game = new LinesGame(pbCenter.Size);
        }

        private void LinesForm_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            timer1.Start();
        }

        private void pbCenter_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(pbCenter.BackColor);
            game.DrawBackground(g);
            game.DrawTokens(g);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            pbCenter.Invalidate();
        }

        private void pbCenter_MouseClick(object sender, MouseEventArgs e)
        {
            game.clickHandler(e.X, e.Y);
        }
    }
}


//g.DrawString(pbCenter.Width.ToString() + pbCenter.Height.ToString(), new Font(FontFamily.GenericMonospace, 20), new SolidBrush(Color.Red), 200f, 200f); XXX 