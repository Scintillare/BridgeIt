using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using QuickGraph;
using QuickGraph.Algorithms;

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
        private readonly List<Vertex> _availableVertices;

        private Size _borderSize; //TODO get set?
        private Vertex _clickedVertex;
        private readonly bool _isAgainstPc; // TODO 
        private UndirectedGraph<Vertex, Edge<Vertex>> _moveHistoryP1;
        private UndirectedGraph<Vertex, Edge<Vertex>> _moveHistoryP2;
        private BidirectionalGraph<Vertex, Edge<Vertex>> _moveMapP1;
        private BidirectionalGraph<Vertex, Edge<Vertex>> _moveMapP2;
        private Rectangle[,] _p1Tokens;
        private Rectangle[,] _p2Tokens;
        public bool IsGameOver;

        public LinesGame(Size borderSize, bool againstPc)
        {
            IsGameOver = false;
            MoveCount = 0;
            _isAgainstPc = againstPc;
            _borderSize = borderSize;
            _clickedVertex = null;
            _availableVertices = new List<Vertex>();
            if (_isAgainstPc) InitAi(); //BUG 
            InitGraphs();
            InitTokens();
        }

        public int MoveCount { get; private set; }

        private void InitAi()
        {
            //BUG
        }

        private void InitGraphs()
        {
            _moveHistoryP1 = new UndirectedGraph<Vertex, Edge<Vertex>>();
            _moveHistoryP2 = new UndirectedGraph<Vertex, Edge<Vertex>>();
            _moveMapP1 = new BidirectionalGraph<Vertex, Edge<Vertex>>();
            _moveMapP2 = new BidirectionalGraph<Vertex, Edge<Vertex>>();

            for (var r = 0; r < Config.POINT_NUMBER_1; ++r)
            for (var c = 0; c < Config.POINT_NUMBER_0; ++c)
            {
                if (r + 1 != Config.POINT_NUMBER_1) // horizontal line 
                {
                    _moveMapP1.AddVerticesAndEdge(
                        new Edge<Vertex>(new Vertex(r, c), new Vertex(r + 1, c)));
                    _moveMapP1.AddEdge(
                        new Edge<Vertex>(new Vertex(r + 1, c), new Vertex(r, c)));
                    _moveMapP2.AddVerticesAndEdge(
                        new Edge<Vertex>(new Vertex(c, r), new Vertex(c, r + 1)));
                    _moveMapP2.AddEdge(
                        new Edge<Vertex>(new Vertex(c, r + 1), new Vertex(c, r)));
                }

                if (c + 1 != Config.POINT_NUMBER_0) //vertical line
                {
                    _moveMapP1.AddVerticesAndEdge(
                        new Edge<Vertex>(new Vertex(r, c), new Vertex(r, c + 1)));
                    _moveMapP1.AddEdge(
                        new Edge<Vertex>(new Vertex(r, c + 1), new Vertex(r, c)));
                    _moveMapP2.AddVerticesAndEdge(
                        new Edge<Vertex>(new Vertex(c, r), new Vertex(c + 1, r)));
                    _moveMapP2.AddEdge(
                        new Edge<Vertex>(new Vertex(c + 1, r), new Vertex(c, r)));
                }
            }
        }

        public bool IsFirstPlayerMove()
        {
            return MoveCount % 2 == 0;
        }

        private Tuple<Rectangle, Vertex> ClickedRectangleAndVertex(int x, int y)
        {
            /*Функция возвращает кортеж только если игрок выбрал фигуру своего цвета*/
            var p = new Point(x, y);
            var arr = IsFirstPlayerMove() ? ref _p1Tokens : ref _p2Tokens;
            for (var i = 0; i < arr.GetLength(0); ++i)
            for (var j = 0; j < arr.GetLength(1); ++j)
                if (arr[i, j].Contains(p))
                    return Tuple.Create(arr[i, j], new Vertex(i, j));

            throw new KeyNotFoundException();
        }

        private void InitTokens()
        {
            _p1Tokens = new Rectangle[Config.POINT_NUMBER_1, Config.POINT_NUMBER_0];
            _p2Tokens = new Rectangle[Config.POINT_NUMBER_0, Config.POINT_NUMBER_1];

            var cellSize = _borderSize.Width / Config.CELL_NUMBER;
            var shiftSize = cellSize * 2;

            for (var row = 0; row < _p1Tokens.GetLength(0); ++row)
            {
                var y = cellSize + row * shiftSize;
                for (var col = 0; col < _p1Tokens.GetLength(1); ++col)
                {
                    var x = col * shiftSize + shiftSize;
                    _p1Tokens[row, col] = new Rectangle(x - Config.POINT_RADIUS,
                        y - Config.POINT_RADIUS,
                        Config.POINT_RADIUS * 2,
                        Config.POINT_RADIUS * 2);
                }
            }

            for (var row = 0; row < _p2Tokens.GetLength(0); ++row)
            {
                var y = shiftSize + row * shiftSize;
                for (var col = 0; col < _p2Tokens.GetLength(1); ++col)
                {
                    var x = col * shiftSize + cellSize;
                    _p2Tokens[row, col] = new Rectangle(x - Config.POINT_RADIUS,
                        y - Config.POINT_RADIUS,
                        Config.POINT_RADIUS * 2,
                        Config.POINT_RADIUS * 2);
                }
            }
        }

        public void DrawBackground(Graphics graphics)
        {
            _drawBorders(graphics);
            var cellSize = _borderSize.Width / Config.CELL_NUMBER;
            var pen = new Pen(Config.GRID_COLOR, Config.GRID_LW);
            for (var p = 0; p <= _borderSize.Width; p += cellSize)
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
            var p1B = new SolidBrush(Config.PLAYER1_COLOR);
            var p2B = new SolidBrush(Config.PLAYER2_COLOR);
            foreach (var token in _p1Tokens) graphics.FillEllipse(p1B, token);

            foreach (var token in _p2Tokens) graphics.FillEllipse(new SolidBrush(Config.PLAYER2_COLOR), token);

            if (_clickedVertex == null) return;
            var p = 2;
            var enlargedR = IsFirstPlayerMove()
                ? _p1Tokens[_clickedVertex.Item1, _clickedVertex.Item2]
                : _p2Tokens[_clickedVertex.Item1, _clickedVertex.Item2];
            enlargedR = new Rectangle(enlargedR.X - p, enlargedR.Y - p, enlargedR.Width + p * 2,
                enlargedR.Height + p * 2);
            graphics.FillEllipse(IsFirstPlayerMove() ? p1B : p2B, enlargedR);

            if (_availableVertices.Count == 0) return;
            var pb = new SolidBrush(Config.HL_COLOR);
            foreach (var (row, col) in _availableVertices)
                graphics.FillEllipse(pb, IsFirstPlayerMove() ? _p1Tokens[row, col] : _p2Tokens[row, col]);
        }

        public void DrawLines(Graphics graphics)
        {
            var pen1 = new Pen(Config.PLAYER1_COLOR, Config.LINE_WIDTH);
            foreach (var edge in _moveHistoryP1.Edges)
            {
                var rect1 = _p1Tokens[edge.Source.Item1, edge.Source.Item2];
                var rect2 = _p1Tokens[edge.Target.Item1, edge.Target.Item2];
                var p1 = new Point(rect1.X + rect1.Width / 2, rect1.Y + rect1.Height / 2);
                var p2 = new Point(rect2.X + rect2.Width / 2, rect2.Y + rect1.Height / 2);
                graphics.DrawLine(pen1, p1, p2);
            }

            var pen2 = new Pen(Config.PLAYER2_COLOR, Config.LINE_WIDTH);
            foreach (var edge in _moveHistoryP2.Edges)
            {
                var rect1 = _p2Tokens[edge.Source.Item1, edge.Source.Item2];
                var rect2 = _p2Tokens[edge.Target.Item1, edge.Target.Item2];
                var p1 = new Point(rect1.X + rect1.Width / 2, rect1.Y + rect1.Height / 2);
                var p2 = new Point(rect2.X + rect2.Width / 2, rect2.Y + rect1.Height / 2);
                graphics.DrawLine(pen2, p1, p2);
            }
        }


        private bool _isGameOver()
        {
            // так как проверка выолняяется после хода, то используется отрицание очередности ходов
            var earlyStop = !IsFirstPlayerMove()
                ? _moveHistoryP1.EdgeCount < Config.POINT_NUMBER_0
                : _moveHistoryP2.EdgeCount < Config.POINT_NUMBER_0;
            if (earlyStop) return false;

            Func<Edge<Vertex>, double> edgeCost = e => 1;
            IEnumerable<Edge<Vertex>> path;

            var sourcePoints = new List<Vertex>();
            var targetPoints = new List<Vertex>();
            int start = 0, end = Config.POINT_NUMBER_1 - 1;
            var graph = !IsFirstPlayerMove() ? _moveHistoryP1 : _moveHistoryP2;
            foreach (var point in graph.Vertices)
            {
                var item = !IsFirstPlayerMove() ? point.Item1 : point.Item2;
                if (item == start) sourcePoints.Add(point);
                if (item == end) targetPoints.Add(point);
            }

            if (sourcePoints.Count == 0 || targetPoints.Count == 0) return false;

            return sourcePoints.Select(source => graph.ShortestPathsDijkstra(edgeCost, source)).Any(tryGetPaths =>
                targetPoints.Any(target => tryGetPaths(target, out path)));
        }

        public void clickHandler(int x, int y)
        {
            Vertex v;
            try
            {
                (_, v) = ClickedRectangleAndVertex(x, y);
            }
            catch (KeyNotFoundException)
            {
                return;
            }

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
                    IsGameOver = true;
                //TODO обвести линию
            }
            else
            {
                _clickedVertex = v;
                _updateAvailableMoves();
            }
        }


        private void _updateAvailableMoves()
        {
            if (_availableVertices.Count != 0) _availableVertices.Clear();
            var mapGraph = IsFirstPlayerMove() ? _moveMapP1 : _moveMapP2;
            var histGraph = IsFirstPlayerMove() ? _moveHistoryP1 : _moveHistoryP2;
            foreach (var edge in mapGraph.OutEdges(_clickedVertex))
            {
                if (histGraph.EdgeCount != 0)
                    if (histGraph.ContainsVertex(edge.Target) && histGraph.ContainsVertex(_clickedVertex))
                        if (histGraph.ContainsEdge(edge.Target, _clickedVertex))
                            continue;
                _availableVertices.Add(edge.Target);
            }
        }


        private void _doMove(Vertex targetVertex)
        {
            Vertex p1, p2;
            if (IsFirstPlayerMove())
            {
                if (_moveMapP1.ContainsEdge(_clickedVertex, targetVertex))
                {
                    _moveHistoryP1.AddVerticesAndEdge(new Edge<Vertex>(_clickedVertex, targetVertex));
                    if (_clickedVertex.Item1 == targetVertex.Item1) // horizontal line
                    {
                        p2 = _clickedVertex.Item2 < targetVertex.Item2 ? targetVertex : _clickedVertex;
                        p1 = new Vertex(p2.Item1 - 1, p2.Item2);
                    }
                    else //vertical line
                    {
                        p1 = _clickedVertex.Item1 < targetVertex.Item1 ? _clickedVertex : targetVertex;
                        p2 = new Vertex(p1.Item1, p1.Item2 + 1);
                    }

                    _moveMapP2.RemoveEdgeIf(edge =>
                        edge.Source.Equals(p1) && edge.Target.Equals(p2) ||
                        edge.Source.Equals(p2) && edge.Target.Equals(p1));
                }
            }
            else
            {
                if (_moveMapP2.ContainsEdge(_clickedVertex, targetVertex))
                {
                    _moveHistoryP2.AddVerticesAndEdge(new Edge<Vertex>(_clickedVertex, targetVertex));

                    if (_clickedVertex.Item1 == targetVertex.Item1) // horizontal line
                    {
                        p1 = _clickedVertex.Item2 < targetVertex.Item2 ? _clickedVertex : targetVertex;
                        p2 = new Vertex(p1.Item1 + 1, p1.Item2);
                    }
                    else //vertical line
                    {
                        p2 = _clickedVertex.Item1 < targetVertex.Item1 ? targetVertex : _clickedVertex;
                        p1 = new Vertex(p2.Item1, p2.Item2 - 1);
                    }

                    _moveMapP1.RemoveEdgeIf(edge =>
                        edge.Source.Equals(p1) && edge.Target.Equals(p2) ||
                        edge.Source.Equals(p2) && edge.Target.Equals(p1));
                }
            }

            _availableVertices.Clear();
            MoveCount += 1;
            _clickedVertex = null;
        }
    }
}