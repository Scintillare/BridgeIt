using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
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

        private bool isGameStarted;
        private Rectangle header;
        private Rectangle[] startButtons;
        private int hoveredButton;

        public LinesForm()
        {
            InitializeComponent();
            this.Size = Config.WINDOW_SIZE;
            if (pbCenter.Width <= pbCenter.Height)
                this.Width += pbCenter.Height - pbCenter.Width;
            else
                this.Height += pbCenter.Width - pbCenter.Height;
            InitStartScreen();
        }

        private void InitStartScreen()
        {
            isGameStarted = false;
            hoveredButton = -1;
            header = new Rectangle(pbCenter.Location, new Size(pbCenter.Width, pbCenter.Height / 5));
            startButtons = new[]
            {
                new Rectangle(header.X, header.Bottom, header.Width / 2, pbCenter.Height - header.Bottom),
                new Rectangle(header.Width / 2, header.Bottom, header.Width / 2, pbCenter.Height - header.Bottom)
            };
        }

        private void ShowStartScreen(Graphics graphics)
        {
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            Font font = new Font("Segoe Print", Config.FONT_SIZE);


            graphics.DrawString("Режим игры:", font, new SolidBrush(Config.HL_COLOR), header, stringFormat);
            graphics.DrawRectangle(Pens.Black, header);

            graphics.DrawString("Против игрока", font, hoveredButton == 0 ? Brushes.Red : Brushes.Black,
                startButtons[0], stringFormat);
            graphics.DrawRectangle(Pens.Black, startButtons[0]);

            graphics.DrawString("Против компьютера", font, hoveredButton == 1 ? Brushes.Red : Brushes.Black,
                startButtons[1], stringFormat);
            graphics.DrawRectangle(Pens.Black, startButtons[1]);
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
            while (!isGameStarted)
            {
                ShowStartScreen(g);
                return;
            }

            game.DrawBackground(g);
            game.DrawLines(g);
            game.DrawTokens(g);
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            pbCenter.Invalidate();
            if (!isGameStarted) return;
            if (!game.isGameOver) return;
            timer1.Stop();
            WinForm winDialog = new WinForm(!game.IsFirstPlayerMove(), game.MoveCount);

            // Show testDialog as a modal dialog and determine if DialogResult = OK.
            if ((winDialog.ShowDialog() == DialogResult.Retry))
            {
                isGameStarted = false;
                timer1.Start();
            }
            else 
            {
            }
            winDialog.Dispose();
        }

        private void pbCenter_MouseClick(object sender, MouseEventArgs e)
        {
            if (!isGameStarted)
            {
                bool agpc;
                if (hoveredButton == 0) agpc = false;
                else if (hoveredButton == 1) agpc = true;
                else return;
                isGameStarted = true;
                game = new LinesGame(pbCenter.Size, agpc);
                return;
            }
            if (!game.isGameOver)
            {
                game.clickHandler(e.X, e.Y);
            }
        }
        
        private void pbCenter_MouseMove(object sender, MouseEventArgs me)
        {
            if (header.Contains(me.Location))
            {
                hoveredButton = -1;
                return;
            }

            for (int i = 0; i < startButtons.Length; ++i)
            {
                if (startButtons[i].Contains(me.Location))
                {
                    hoveredButton = i;
                }
            }
        }
    }
}