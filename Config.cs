using System.Drawing;

namespace LinesGame
{
    public static class Config
    {
        public static Size WINDOW_SIZE = new Size(700, 700);
        public static int POINT_RADIUS = 10;
        public static float BORDER_SIZE = 20;
        public static int POINT_NUMBER = 5;
        public static int CELL_NUMBER = 2 + POINT_NUMBER*2;
        public static Color GRID_COLOR = Color.Gray;
        public static int GRID_LW = 2;
        public static Color PLAYER1_COLOR = Color.Red;
        public static Color PLAYER2_COLOR = Color.Blue;
    }
}