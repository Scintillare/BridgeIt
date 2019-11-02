using System;
using System.Drawing;
using System.Windows.Forms;

namespace LinesGame
{
    public partial class LinesForm
    {
        private LinesGame _game;
        private Rectangle _header;
        private int _hoveredButton;

        private bool _isGameStarted;
        private Rectangle[] _startButtons;

        public LinesForm()
        {
            InitializeComponent();
            Size = Config.WINDOW_SIZE;
            if (pbCenter.Width <= pbCenter.Height)
                Width += pbCenter.Height - pbCenter.Width;
            else
                Height += pbCenter.Width - pbCenter.Height;
            InitStartScreen();
        }

        private void InitStartScreen()
        {
            _isGameStarted = false;
            _hoveredButton = -1;
            _header = new Rectangle(pbCenter.Location, new Size(pbCenter.Width, pbCenter.Height / 5));
            _startButtons = new[]
            {
                new Rectangle(_header.X, _header.Bottom, _header.Width / 2, pbCenter.Height - _header.Bottom),
                new Rectangle(_header.Width / 2, _header.Bottom, _header.Width / 2, pbCenter.Height - _header.Bottom)
            };
        }

        private void ShowStartScreen(Graphics graphics)
        {
            var stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center
            };
            var font = new Font("Segoe Print", Config.FONT_SIZE);

            graphics.DrawString("Режим игры:", font, new SolidBrush(Config.HL_COLOR), _header, stringFormat);
            graphics.DrawRectangle(Pens.Black, _header);

            graphics.DrawString("Против игрока", font, _hoveredButton == 0 ? Brushes.Red : Brushes.Black,
                _startButtons[0], stringFormat);
            graphics.DrawRectangle(Pens.Black, _startButtons[0]);

            graphics.DrawString("Против компьютера", font, _hoveredButton == 1 ? Brushes.Red : Brushes.Black,
                _startButtons[1], stringFormat);
            graphics.DrawRectangle(Pens.Black, _startButtons[1]);
        }

        private void LinesForm_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            timer1.Start();
        }

        private void pbCenter_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(pbCenter.BackColor);
            while (!_isGameStarted)
            {
                ShowStartScreen(g);
                return;
            }

            _game.DrawBackground(g);
            _game.DrawLines(g);
            _game.DrawTokens(g);
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            pbCenter.Invalidate();
            if (!_isGameStarted) return;
            if (!_game.IsGameOver) return;
            timer1.Stop();
            var winDialog = new WinForm(!_game.IsFirstPlayerMove(), _game.MoveCount);

            // Show testDialog as a modal dialog and determine if DialogResult = OK.
            if (winDialog.ShowDialog() == DialogResult.Retry)
            {
                _isGameStarted = false;
                timer1.Start();
            }

            winDialog.Dispose();
        }

        private void pbCenter_MouseClick(object sender, MouseEventArgs e)
        {
            if (!_isGameStarted)
            {
                bool agpc;
                switch (_hoveredButton)
                {
                    case 0:
                        agpc = false;
                        break;
                    case 1:
                        agpc = true;
                        break;
                    default:
                        return;
                }
                _isGameStarted = true;
                _game = new LinesGame(pbCenter.Size, agpc);
                return;
            }

            if (!_game.IsGameOver) _game.clickHandler(e.X, e.Y);
        }

        private void pbCenter_MouseMove(object sender, MouseEventArgs me)
        {
            if (_header.Contains(me.Location))
            {
                _hoveredButton = -1;
                return;
            }

            for (var i = 0; i < _startButtons.Length; ++i)
                if (_startButtons[i].Contains(me.Location))
                    _hoveredButton = i;
        }
    }
}