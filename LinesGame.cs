using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Algorithms.ShortestPath;

namespace LinesGame
{
    public class Vertex : Tuple<int, int>
    {
        public Vertex(int row, int col) : base(row, col)
        {
        }
    }


    public class LinesGame
    {
        private UndirectedGraph<Vertex, UndirectedEdge<Vertex>> _moveHistoryP1;
        private UndirectedGraph<Vertex, UndirectedEdge<Vertex>> _moveHistoryP2;
        private UndirectedGraph<Vertex, UndirectedEdge<Vertex>> _moveMapP1;
        private UndirectedGraph<Vertex, UndirectedEdge<Vertex>> _moveMapP2;
        private int _moveCount;
        private bool _isAgainstPc; // TODO 
        public bool isGameOver;


        private Size _borderSize; //TODO get set?
        private Rectangle[,] _p1Tokens;
        private Rectangle[,] _p2Tokens;
        private Vertex _clickedVertex;
        private List<Vertex> _availableVertices;

        public LinesGame(Size borderSize, bool againstPC)
        {
            isGameOver = false;
            _moveCount = 0;
            _isAgainstPc = againstPC;
            _borderSize = borderSize;
            _clickedVertex = null;
            if (_isAgainstPc) InitAI(); //BUG 
            InitGraphs();
            InitTokens();
        }

        public void InitAI()
        {
            //BUG
        }

        private void InitGraphs()
        {
            _moveHistoryP1 = new UndirectedGraph<Vertex, UndirectedEdge<Vertex>>();
            _moveHistoryP2 = new UndirectedGraph<Vertex, UndirectedEdge<Vertex>>();
            _moveMapP1 = new UndirectedGraph<Vertex, UndirectedEdge<Vertex>>();
            _moveMapP2 = new UndirectedGraph<Vertex, UndirectedEdge<Vertex>>();

            for (int r = 0; r < Config.POINT_NUMBER_1; ++r)
            {
                for (int c = 0; c < (Config.POINT_NUMBER_0); ++c)
                {
                    if (r + 1 != Config.POINT_NUMBER_1) // horizontal line 
                    {
                        _moveMapP1.AddVerticesAndEdge(
                            new UndirectedEdge<Vertex>(new Vertex(r, c), new Vertex(r + 1, c)));
                        _moveMapP2.AddVerticesAndEdge(
                            new UndirectedEdge<Vertex>(new Vertex(c, r), new Vertex(c, r + 1)));
                    }

                    if (c + 1 != Config.POINT_NUMBER_0) //vertical line
                    {
                        _moveMapP1.AddVerticesAndEdge(
                            new UndirectedEdge<Vertex>(new Vertex(r, c), new Vertex(r, c + 1)));
                        _moveMapP2.AddVerticesAndEdge(
                            new UndirectedEdge<Vertex>(new Vertex(c, r), new Vertex(c + 1, r)));
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
            var arr = IsFirstPlayerMove() ? ref _p1Tokens : ref _p2Tokens;
            for (var i = 0; i < arr.GetLength(0); ++i)
            {
                for (var j = 0; j < arr.GetLength(1); ++j)
                {
                    if (arr[i, j].Contains(p))
                    {
                        return Tuple.Create(arr[i, j], new Vertex(i, j));
                    }
                }
            }

            throw new KeyNotFoundException();
        }

        private void InitTokens()
        {
            _p1Tokens = new Rectangle[Config.POINT_NUMBER_1, Config.POINT_NUMBER_0];
            _p2Tokens = new Rectangle[Config.POINT_NUMBER_0, Config.POINT_NUMBER_1];

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

        public void DrawTokens(Graphics graphics)
        {
            SolidBrush p1b = new SolidBrush(Config.PLAYER1_COLOR);
            SolidBrush p2b = new SolidBrush(Config.PLAYER2_COLOR);
            foreach (var token in _p1Tokens)
            {
                graphics.FillEllipse(p1b, token);
            }

            foreach (var token in _p2Tokens)
            {
                graphics.FillEllipse(new SolidBrush(Config.PLAYER2_COLOR), token);
            }

            if (_clickedVertex == null) return;
            var p = 2;
            var enlargedR = IsFirstPlayerMove()
                ? _p1Tokens[_clickedVertex.Item1, _clickedVertex.Item2]
                : _p2Tokens[_clickedVertex.Item1, _clickedVertex.Item2];
            enlargedR = new Rectangle(enlargedR.X - p, enlargedR.Y - p, enlargedR.Width + p * 2,
                enlargedR.Height + p * 2);
            graphics.FillEllipse((IsFirstPlayerMove() ? p1b : p2b), enlargedR);
        }

        public void DrawLines(Graphics graphics)
        {
            foreach (var edge in _moveHistoryP1.Edges)
            {
                Pen pen = new Pen(Config.PLAYER1_COLOR, 5);
                var rect1 = _p1Tokens[edge.Source.Item1, edge.Source.Item2];
                var rect2 = _p1Tokens[edge.Target.Item1, edge.Target.Item2];
                var p1 = new Point(rect1.X + rect1.Width / 2, rect1.Y + rect1.Height / 2);
                var p2 = new Point(rect2.X + rect2.Width / 2, rect2.Y + rect1.Height / 2);
                graphics.DrawLine(pen, p1, p2);
            }

            foreach (var edge in _moveHistoryP2.Edges)
            {
                Pen pen = new Pen(Config.PLAYER2_COLOR, 5);
                var rect1 = _p2Tokens[edge.Source.Item1, edge.Source.Item2];
                var rect2 = _p2Tokens[edge.Target.Item1, edge.Target.Item2];
                var p1 = new Point(rect1.X + rect1.Width / 2, rect1.Y + rect1.Height / 2);
                var p2 = new Point(rect2.X + rect2.Width / 2, rect2.Y + rect1.Height / 2);
                graphics.DrawLine(pen, p1, p2);
            }
        }


        private bool _isGameOver()
        {
            // так как проверка выолняяется после хода, то используется отрицание очередности ходов
            bool earlyStop = !IsFirstPlayerMove()
                ? _moveHistoryP1.EdgeCount < Config.POINT_NUMBER_0
                : _moveHistoryP2.EdgeCount < Config.POINT_NUMBER_0;
            if (earlyStop) return false;

            Func<UndirectedEdge<Vertex>, double> edgeCost = e => 1;
            IEnumerable<UndirectedEdge<Vertex>> path;

            List<Vertex> sourcePoints = new List<Vertex>();
            List<Vertex> targetPoints = new List<Vertex>();
            int start = 0, end = Config.POINT_NUMBER_1 - 1;
            var graph = !IsFirstPlayerMove() ? _moveHistoryP1 : _moveHistoryP2;
            foreach (var point in graph.Vertices)
            {
                var item = !IsFirstPlayerMove() ? point.Item1 : point.Item2;
                if (item == start) sourcePoints.Add(point);
                if (item == end) targetPoints.Add(point);
            }

            if (sourcePoints.Count == 0 || targetPoints.Count == 0) return false;

            foreach (var source in sourcePoints)
            {
                var tryGetPaths = graph.ShortestPathsDijkstra(edgeCost, source);
                foreach (var target in targetPoints)
                {
                    if (tryGetPaths(target, out path))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void clickHandler(int x, int y)
        {
            try
            {
                (Rectangle rect, Vertex v) = ClickedRectangleAndVertex(x, y);
                if (_clickedVertex != null)
                {
                    if (_clickedVertex.Equals(v))
                    {
                        _clickedVertex = null;
                        return;
                    }

                    if (!_availableVertices.Contains(v)) return;
                    _doMove(v);
                    if (_isGameOver())
                    {
                        isGameOver = true;
                        //TODO обвести линию
                    }
                }
                else
                {
                    _clickedVertex = v;
                    _availableVertices = _getAvailableMoves(v);
                    //TODO highlight indices
                }
            }
            catch (KeyNotFoundException e)
            {
                return;
            }
        }

        private List<Vertex> _getAvailableMoves(Vertex vertex)
        {
            List<Vertex> list = new List<Vertex>();
            var edges = IsFirstPlayerMove() ? _moveMapP1.AdjacentEdges(vertex) : _moveMapP2.AdjacentEdges(vertex);

            foreach (var edge in edges)
            {
                list.Add(edge.Target);
            }

            return list;
        }


        private void _doMove(Vertex v)
        {
            if (IsFirstPlayerMove())
            {
                if (_moveMapP1.ContainsEdge(_clickedVertex, v) || _moveMapP1.ContainsEdge(v, _clickedVertex))
                {
                    _moveHistoryP1.AddVerticesAndEdge(new UndirectedEdge<Vertex>(_clickedVertex, v));
                }
            }
            else
            {
                if (_moveMapP2.ContainsEdge(_clickedVertex, v) || _moveMapP2.ContainsEdge(v, _clickedVertex))
                {
                    _moveHistoryP2.AddVerticesAndEdge(new UndirectedEdge<Vertex>(_clickedVertex, v));
                }
            }
            // TODO FIXME BUG

            _moveCount += 1;
            _clickedVertex = null;
        }
    }
}