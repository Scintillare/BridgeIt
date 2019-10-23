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
        
        
        public LinesForm()
        {
            InitializeComponent();
            this.Size = Config.WINDOW_SIZE;
            if (pbCenter.Width <= pbCenter.Height)
                this.Width += (pbCenter.Height - pbCenter.Width);
            else
                this.Height += (pbCenter.Width - pbCenter.Height);
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
            _drawBackground(g);
            
        }

        private void _drawBackground(Graphics g)
        {
            g.DrawLine(new Pen(Config.PLAYER1_COLOR, Config.BORDER_SIZE), new Point(0, 0),
                new Point(pbCenter.Width, 0));
            g.DrawLine(new Pen(Config.PLAYER1_COLOR, Config.BORDER_SIZE), new Point(0, pbCenter.Height),
                new Point(pbCenter.Width, pbCenter.Height));
            g.DrawLine(new Pen(Config.PLAYER2_COLOR, Config.BORDER_SIZE), new Point(0, 0),
                new Point(0, pbCenter.Height));
            g.DrawLine(new Pen(Config.PLAYER2_COLOR, Config.BORDER_SIZE), new Point(pbCenter.Width, 0),
                new Point(pbCenter.Width, pbCenter.Height));
            int cellSize = pbCenter.Width / Config.CELL_NUMBER;
            Pen pen = new Pen(Config.GRID_COLOR, Config.GRID_LW);
            for (int p = 0; p <= pbCenter.Width; p += cellSize)
            {
                g.DrawLine(pen, new Point(p, 0), new Point(p, pbCenter.Height)); // vertical line 
                g.DrawLine(pen, new Point(0, p), new Point(pbCenter.Width, p)); // horizontal line
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            pbCenter.Invalidate();
        }
    }
}


//g.DrawString(pbCenter.Width.ToString() + pbCenter.Height.ToString(), new Font(FontFamily.GenericMonospace, 20), new SolidBrush(Color.Red), 200f, 200f); XXX 