using System.Drawing;

namespace LinesGame
{
    public static class Config
    {
        public static Size WINDOW_SIZE = new Size(700, 700);
        public const int FONT_SIZE = 30;
        public const int LINE_WIDTH = 5;
        public const int POINT_RADIUS = 10;
        public const float BORDER_SIZE = 20;
        public const int POINT_NUMBER_0 = 5;
        public const int POINT_NUMBER_1 = 6;
        public const int CELL_NUMBER = 2 + POINT_NUMBER_0 * 2;
        public static Color GRID_COLOR = Color.Gray;
        public const int GRID_LW = 2;
        public static Color PLAYER1_COLOR = Color.Blue;
        public static Color PLAYER2_COLOR = Color.Red;
        public static Color HL_COLOR = Color.Green;
    }
}