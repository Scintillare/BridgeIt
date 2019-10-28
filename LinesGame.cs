using System.Collections.Generic;
using System.Drawing;

namespace LinesGame
{
    public struct Line
    {
        private Point startPoint { get; set; }
        private Point endPoint { get; set; }
        
        public Line(Point firstPoint, Point secondPoint)
        {
            this.startPoint = firstPoint;
            this.endPoint = secondPoint;
        }
    }
    
    public class LinesGame
    {
        
        private Size _borderSize; //TODO get set?
        private Rectangle[] p1_points;
        private Rectangle[] p2_points;
        private bool isFirstPointClicked;
        private Point firstPoint;
        private List<Line> lines;
        private int moveCount;

        public LinesGame(Size borderSize)
        {
            moveCount = 0;
            _borderSize = borderSize;
            isFirstPointClicked = false;
            lines = new List<Line>();
            initPoints();
        }


        private void initPoints()
        {
            p1_points = new Rectangle[Config.POINT_NUMBER * (Config.POINT_NUMBER + 1)];
            p2_points = new Rectangle[(Config.POINT_NUMBER + 1) * Config.POINT_NUMBER];

            int cellSize = _borderSize.Width / Config.CELL_NUMBER;
            int shiftSize = cellSize * 2;

            for (int pc = 0; pc < (Config.POINT_NUMBER + 1); ++pc)
            {
                int y = cellSize + (pc * shiftSize);
                for (int pr = 0; pr < Config.POINT_NUMBER; ++pr)
                {
                    int x = (pr * shiftSize) + shiftSize;
                    p1_points[pc * Config.POINT_NUMBER + pr] = new Rectangle((x - Config.POINT_RADIUS),
                        (y - Config.POINT_RADIUS),
                        Config.POINT_RADIUS * 2,
                        Config.POINT_RADIUS * 2);
                }
            }
            for (int pc = 0; pc < Config.POINT_NUMBER; ++pc)
            {
                int y = shiftSize + (pc * shiftSize);
                for (int pr = 0; pr < (Config.POINT_NUMBER + 1); ++pr)
                {
                    int x = (pr * shiftSize) + cellSize;
                    p2_points[pc  + pr* Config.POINT_NUMBER] = new Rectangle((x - Config.POINT_RADIUS),
                        (y - Config.POINT_RADIUS),
                        Config.POINT_RADIUS * 2,
                        Config.POINT_RADIUS * 2);
                }
            }
        }

        public void drawBackground(Graphics graphics)
        {
            _drawBorders(graphics);

            int cellSize = _borderSize.Width / Config.CELL_NUMBER;
            Pen pen = new Pen(Config.GRID_COLOR, Config.GRID_LW);
            for (int p = 0; p <= _borderSize.Width; p += cellSize)
            {
                graphics.DrawLine(pen, new Point(p, 0), new Point(p, _borderSize.Height)); // vertical line 
                graphics.DrawLine(pen, new Point(0, p), new Point(_borderSize.Width, p)); // horizontal line
            }
        }

        private void _drawBorders(Graphics graphics)
        {
            graphics.DrawLine(new Pen(Config.PLAYER1_COLOR, Config.BORDER_SIZE), new Point(0, 0),
                new Point(_borderSize.Width, 0));
            graphics.DrawLine(new Pen(Config.PLAYER1_COLOR, Config.BORDER_SIZE), new Point(0, _borderSize.Height),
                new Point(_borderSize.Width, _borderSize.Height));
            graphics.DrawLine(new Pen(Config.PLAYER2_COLOR, Config.BORDER_SIZE), new Point(0, 0),
                new Point(0, _borderSize.Height));
            graphics.DrawLine(new Pen(Config.PLAYER2_COLOR, Config.BORDER_SIZE), new Point(_borderSize.Width, 0),
                new Point(_borderSize.Width, _borderSize.Height));
        }

        public void drawPoints(Graphics graphics)
        {
            foreach (var rect in p1_points)
            {
                graphics.FillEllipse(new SolidBrush(Config.PLAYER1_COLOR), rect); //TODO FIX
            }

            foreach (var rect in p2_points)
            {
                graphics.FillEllipse(new SolidBrush(Config.PLAYER2_COLOR), rect); //TODO FIX
            }
        }

        public void clickHandler(int x, int y)
        {
            if (isFirstPointClicked)
            {
                lines.Add(new Line(firstPoint, getPoint(x, y)));
                moveCount += 1;
            }
            else
            {
                isFirstPointClicked = true;
                firstPoint = getPoint(x, y);
                //TODO highlight point
            }
        }

        private Point getPoint(int x, int y)
        {
            foreach (var VARIABLE in p1_points)
            {
                
            }
        }


        public void drawPlayersLines(Graphics graphics)
        {
            foreach (var line in lines)
            {
                graphics.DrawLine();
            }
            throw new System.NotImplementedException();
        }
    }
}