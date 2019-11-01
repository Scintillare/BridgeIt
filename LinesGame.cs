using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
        private UndirectedGraph<Vertex, Edge<Vertex>> _moveHistoryP1;
        private UndirectedGraph<Vertex, Edge<Vertex>> _moveHistoryP2;
        private BidirectionalGraph<Vertex, Edge<Vertex>> _moveMapP1;
        private BidirectionalGraph<Vertex, Edge<Vertex>> _moveMapP2;
        private int _moveCount;
        private bool _isAgainstPc; // TODO 
        public bool isGameOver;
        
        public int MoveCount => _moveCount;

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
            _availableVertices = new List<Vertex>();
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
            _moveHistoryP1 = new UndirectedGraph<Vertex, Edge<Vertex>>();
            _moveHistoryP2 = new UndirectedGraph<Vertex, Edge<Vertex>>();
            _moveMapP1 = new BidirectionalGraph<Vertex, Edge<Vertex>>();
            _moveMapP2 = new BidirectionalGraph<Vertex, Edge<Vertex>>();

            for (int r = 0; r < Config.POINT_NUMBER_1; ++r)
            {
                for (int c = 0; c < Config.POINT_NUMBER_0; ++c)
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
        }

        public bool IsFirstPlayerMove()
        {
            return _moveCount % 2 == 0;
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
                int y = cellSize + row * shiftSize;
                for (int col = 0; col < _p1Tokens.GetLength(1); ++col)
                {
                    int x = col * shiftSize + shiftSize;
                    _p1Tokens[row, col] = new Rectangle(x - Config.POINT_RADIUS,
                        y - Config.POINT_RADIUS,
                        Config.POINT_RADIUS * 2,
                        Config.POINT_RADIUS * 2);
                }
            }

            for (int row = 0; row < _p2Tokens.GetLength(0); ++row)
            {
                int y = shiftSize + row * shiftSize;
                for (int col = 0; col < _p2Tokens.GetLength(1); ++col)
                {
                    int x = col * shiftSize + cellSize;
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
            graphics.FillEllipse(IsFirstPlayerMove() ? p1b : p2b, enlargedR);
                        
            if (_availableVertices.Count == 0) return;
            SolidBrush pb = new SolidBrush(Config.HL_COLOR);
            foreach (var v in _availableVertices)
            {
                if (IsFirstPlayerMove()) graphics.FillEllipse(pb, _p1Tokens[v.Item1, v.Item2]);
                else graphics.FillEllipse(pb, _p2Tokens[v.Item1, v.Item2]);
            }
        }

        public void DrawLines(Graphics graphics)
        {
            Pen pen1 = new Pen(Config.PLAYER1_COLOR, Config.LINE_WIDTH);
            foreach (var edge in _moveHistoryP1.Edges)
            {
                var rect1 = _p1Tokens[edge.Source.Item1, edge.Source.Item2];
                var rect2 = _p1Tokens[edge.Target.Item1, edge.Target.Item2];
                var p1 = new Point(rect1.X + rect1.Width / 2, rect1.Y + rect1.Height / 2);
                var p2 = new Point(rect2.X + rect2.Width / 2, rect2.Y + rect1.Height / 2);
                graphics.DrawLine(pen1, p1, p2);
            }

            Pen pen2 = new Pen(Config.PLAYER2_COLOR, Config.LINE_WIDTH);
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
            bool earlyStop = !IsFirstPlayerMove()
                ? _moveHistoryP1.EdgeCount < Config.POINT_NUMBER_0
                : _moveHistoryP2.EdgeCount < Config.POINT_NUMBER_0;
            if (earlyStop) return false;

            Func<Edge<Vertex>, double> edgeCost = e => 1;
            IEnumerable<Edge<Vertex>> path;

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
            Vertex v;
            try
            {
                (_, v) = ClickedRectangleAndVertex(x, y);
            }
            catch (KeyNotFoundException e)
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
                {
                    isGameOver = true;
                    //TODO обвести линию
                }
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
                {
                    if (histGraph.ContainsVertex(edge.Target) && histGraph.ContainsVertex(_clickedVertex))
                    {
                        if (histGraph.ContainsEdge(edge.Target, _clickedVertex)) continue;
                    }
                }
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
                        (edge.Source.Equals(p1) && edge.Target.Equals(p2) ||
                         edge.Source.Equals(p2) && edge.Target.Equals(p1)));
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
                        (edge.Source.Equals(p1) && edge.Target.Equals(p2) ||
                         edge.Source.Equals(p2) && edge.Target.Equals(p1)));
                }
            }

            _availableVertices.Clear();
            _moveCount += 1;
            _clickedVertex = null;
        }
    }
}