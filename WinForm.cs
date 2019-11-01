using System;
using System.Windows.Forms;

namespace LinesGame
{
    public partial class WinForm : Form
    {
        public WinForm(bool firstPlWin, int moveCount)
        {
            InitializeComponent();
            if (firstPlWin) lbPlayer.Text = "первый игрок";
            else lbPlayer.Text = "второй игрок";
            lbMoveCount.Text = moveCount.ToString();
        }
    }
}