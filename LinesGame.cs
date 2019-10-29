using System;
using System.Collections.Generic;
using System.Drawing;
using QuickGraph;

namespace LinesGame
{
    public class Vertex
    {
        private int Row { get; set; }
        private int Col { get; set; }

        public Vertex(int row, int col)
        {
            this.Row = row;
            this.Col = col;
        }

        public override bool Equals(object o)
        {
            if (ReferenceEquals(this, o)) return true;
            if (ReferenceEquals(this, null)) return false;
            if (ReferenceEquals(this, null)) return false;
            if (this.GetType() != o.GetType()) return false;
            return this.Row == ((Vertex) o).Row && this.Col == ((Vertex) o).Col;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.Row * 397) ^ this.Col;
            }
        }
    }


    public class LinesGame
    {
        private UndirectedGraph<Vertex, UndirectedEdge<Vertex>> _p1Graph;
        private UndirectedGraph<Vertex, UndirectedEdge<Vertex>> _p2Graph;
        private UndirectedGraph<Vertex, UndirectedEdge<Vertex>> _pcFakeGraph;
        private int _moveCount;
        private bool _isAgainstPc; // TODO 


        private Size _borderSize; //TODO get set?
        private Rectangle[,] _p1Tokens;
        private Rectangle[,] _p2Tokens;
        private Vertex _clickedVertex;

        public LinesGame(Size borderSize)
        {
            _moveCount = 0;
            _isAgainstPc = true;
            _borderSize = borderSize;
            _clickedVertex = null;
            InitGraphs();
            InitTokens();
        }

        private void InitGraphs()
        {
            _p1Graph = new UndirectedGraph<Vertex, UndirectedEdge<Vertex>>();
            _p2Graph = new UndirectedGraph<Vertex, UndirectedEdge<Vertex>>();
            for (int r = 0; r < Config.POINT_NUMBER + 1; ++r)
            {
                for (int c = 0; c < Config.POINT_NUMBER; ++c)
                {
                    _p1Graph.AddVertex(new Vertex(r, c));
                    _p2Graph.AddVertex(new Vertex(c, r));
                }
            }

            if (!_isAgainstPc) return;
            _pcFakeGraph = new UndirectedGraph<Vertex, UndirectedEdge<Vertex>>();
            for (int r = 0; r < Config.POINT_NUMBER; ++r)
            {
                for (int c = 0; c < (Config.POINT_NUMBER + 1); ++c)
                {
                    if (r + 1 != Config.POINT_NUMBER)
                    {
                        _pcFakeGraph.AddVerticesAndEdge(new UndirectedEdge<Vertex>(new Vertex(r, c),
                            new Vertex(r + 1, c)));
                    }

                    if (c + 1 != Config.POINT_NUMBER + 1)
                    {
                        _pcFakeGraph.AddVerticesAndEdge(new UndirectedEdge<Vertex>(new Vertex(r, c),
                            new Vertex(r, c + 1)));
                    }
                }
            }
        }

        private bool IsFirstPlayerMove()
        {
            return (_moveCount % 2 == 0);
        }

        private Tuple<Rectangle, Vertex> ClickedRectangleAndVertex(int x, int y)
        {
            /*Функция возвращает кортеж только если игрок выбрал фигуру своего цвета*/
            var p = new Point(x, y);
            if (IsFirstPlayerMove())
            {
                for (var i = 0; i < _p1Tokens.GetLength(0); ++i)
                {
                    for (var j = 0; j < _p1Tokens.GetLength(1); ++j)
                    {
                        if (_p1Tokens[i, j].Contains(p))
                        {
                            return Tuple.Create(_p1Tokens[i, j], new Vertex(i, j));
                        }
                    }
                }
            }
            else
            {
                for (var i = 0; i < _p2Tokens.GetLength(0); ++i)
                {
                    for (var j = 0; j < _p2Tokens.GetLength(1); ++j)
                    {
                        if (_p1Tokens[i, j].Contains(p))
                        {
                            return Tuple.Create(_p1Tokens[i, j], new Vertex(i, j));
                        }
                    }
                }
            }
            throw new KeyNotFoundException();
        }

        private void InitTokens()
        {
            _p1Tokens = new Rectangle[(Config.POINT_NUMBER + 1), Config.POINT_NUMBER];
            _p2Tokens = new Rectangle[Config.POINT_NUMBER, (Config.POINT_NUMBER + 1)];

            int cellSize = _borderSize.Width / Config.CELL_NUMBER;
            int shiftSize = cellSize * 2;

            for (int row = 0; row < _p1Tokens.GetLength(0); ++row)
            {
                int y = cellSize + (row * shiftSize);
                for (int col = 0; col < _p1Tokens.GetLength(1); ++col)
                {
                    int x = (col * shiftSize) + shiftSize;
                    _p1Tokens[row, col] = new Rectangle((x - Config.POINT_RADIUS),
                        (y - Config.POINT_RADIUS),
                        Config.POINT_RADIUS * 2,
                        Config.POINT_RADIUS * 2);
                }
            }

            for (int row = 0; row < _p2Tokens.GetLength(0); ++row)
            {
                int y = shiftSize + (row * shiftSize);
                for (int col = 0; col < _p2Tokens.GetLength(1); ++col)
                {
                    int x = (col * shiftSize) + cellSize;
                    _p2Tokens[row, col] = new Rectangle((x - Config.POINT_RADIUS),
                        (y - Config.POINT_RADIUS),
                        Config.POINT_RADIUS * 2,
                        Config.POINT_RADIUS * 2);
                }
            }
        }

        public void DrawBackground(Graphics graphics)
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


        public void clickHandler(int x, int y)
        {
            try
            {
                (Rectangle, Vertex) rect, v = ClickedRectangleAndVertex(x, y).ToValueTuple();
                _clickedVertex = null;
            }
            catch (KeyNotFoundException e)
            {   
                return;
            }

            //TODO highlight point
        }


        private void DoMove(Vertex p1, Vertex p2)
        {
            //BUG
            if (IsFirstPlayerMove())
            {
                _p1Graph.AddEdge(new UndirectedEdge<Vertex>(p1, p2));
                //TODO brake line in fake graph!!
            }
            else
            {
                _p2Graph.AddEdge(new UndirectedEdge<Vertex>(p1, p2));
            }
        }
    }
}