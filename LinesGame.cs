using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using QuickGraph;
using QuickGraph.Collections;

namespace LinesGame
{
    public class Vertex : Tuple<int, int>
    {
        public Vertex(int row, int col) : base(row, col)
        {
        }
        
        
        public static bool OnHorizontal(Vertex v1, Vertex v2)
        {
            return v1.Item1 == v2.Item1;
        }

        public static bool OnVertical(Vertex v1, Vertex v2)
        {
            return v1.Item2 == v2.Item2;
        }


        public static bool operator ==(Vertex v1, Vertex v2)
        {
            return (v1 != null && v2 != null) && (v1.Item1 == v2.Item1) && v1.Item2 == v2.Item2;
        }

        public static bool operator !=(Vertex v1, Vertex v2)
        {
            return !(v1 == v2);
        }

        public static bool operator >(Vertex v1, Vertex v2)
        { //BUG
            if (v1 == v2) return false;
            return ((Vertex.OnHorizontal(v1, v2) && v1.Item2 > v2.Item2) ||
                    (Vertex.OnVertical(v1, v2) && v1.Item1 > v2.Item1));
        }

        public static bool operator <(Vertex v1, Vertex v2)
        { // BUG
            if (v1 == v2) return false;
            return ((Vertex.OnHorizontal(v1, v2) && v1.Item2 < v2.Item2) ||
                    (Vertex.OnVertical(v1, v2) && v1.Item1 < v2.Item1));
        }
    }


    public class LinesGame
    {
        private readonly List<Vertex> _availableVertices;

        private Size _borderSize; //TODO get set?
        private Vertex _clickedVertex;
        public readonly bool _isAgainstPc;
        private AIPlayer ai;
        private UndirectedGraph<Vertex, Edge<Vertex>> _moveHistoryP1;
        private UndirectedGraph<Vertex, Edge<Vertex>> _moveHistoryP2;
        private BidirectionalGraph<Vertex, Edge<Vertex>> _moveMapP1;
        private BidirectionalGraph<Vertex, Edge<Vertex>> _moveMapP2;
        private Rectangle[,] _p1Tokens;
        private Rectangle[,] _p2Tokens;
        public bool IsGameOver;
        public int MoveCount { get; private set; }
        public bool IsFirstPlayerMove { get; private set; }

        public LinesGame(Size borderSize, bool againstPc)
        {
            IsGameOver = false;
            MoveCount = 0;
            IsFirstPlayerMove = true;
            _isAgainstPc = againstPc;
            _borderSize = borderSize;
            _clickedVertex = null;
            _availableVertices = new List<Vertex>();
            InitGraphs();
            InitTokens();
            if (_isAgainstPc) InitAi(); //BUG 
        }


        private void InitAi()
        {
            ai = new AIPlayer(this);
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
            const int p = 2;
            var enlargedR = IsFirstPlayerMove
                ? _p1Tokens[_clickedVertex.Item1, _clickedVertex.Item2]
                : _p2Tokens[_clickedVertex.Item1, _clickedVertex.Item2];
            enlargedR = new Rectangle(enlargedR.X - p, enlargedR.Y - p, enlargedR.Width + p * 2,
                enlargedR.Height + p * 2);
            graphics.FillEllipse(IsFirstPlayerMove ? p1B : p2B, enlargedR);

            if (_availableVertices.Count == 0) return;
            var pb = new SolidBrush(Config.HL_COLOR);
            foreach (var (row, col) in _availableVertices)
                graphics.FillEllipse(pb, IsFirstPlayerMove ? _p1Tokens[row, col] : _p2Tokens[row, col]);
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

        public void increaseMoveCount()
        {
            ++MoveCount;
            IsFirstPlayerMove = !IsFirstPlayerMove;
        }

        private Tuple<Rectangle, Vertex> ClickedRectangleAndVertex(int x, int y)
        {
            /*Функция возвращает кортеж только если игрок выбрал фигуру своего цвета*/
            var p = new Point(x, y);
            var arr = IsFirstPlayerMove ? ref _p1Tokens : ref _p2Tokens;
            for (var i = 0; i < arr.GetLength(0); ++i)
            for (var j = 0; j < arr.GetLength(1); ++j)
                if (arr[i, j].Contains(p))
                    return Tuple.Create(arr[i, j], new Vertex(i, j));

            throw new KeyNotFoundException();
        }

        private bool _isGameOver()
        {
            // так как проверка выолняяется после хода, то используется отрицание очередности ходов
            var earlyStop = !IsFirstPlayerMove
                ? _moveHistoryP1.EdgeCount < Config.POINT_NUMBER_0
                : _moveHistoryP2.EdgeCount < Config.POINT_NUMBER_0;
            if (earlyStop) return false;

            Func<Edge<Vertex>, double> edgeCost = e => 1;
            IEnumerable<Edge<Vertex>> path;

            var sourcePoints = new List<Vertex>();
            var targetPoints = new List<Vertex>();
            int start = 0, end = Config.POINT_NUMBER_1 - 1;
            var graph = !IsFirstPlayerMove ? _moveHistoryP1 : _moveHistoryP2;
            foreach (var point in graph.Vertices)
            {
                var item = !IsFirstPlayerMove ? point.Item1 : point.Item2;
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

            clickHandler(v);
        }

        public void clickHandler(Vertex v)
        {
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
            var mapGraph = IsFirstPlayerMove ? _moveMapP1 : _moveMapP2;
            var histGraph = IsFirstPlayerMove ? _moveHistoryP1 : _moveHistoryP2;
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
            var mmap = IsFirstPlayerMove ? _moveMapP1 : _moveMapP2;
            var hist = IsFirstPlayerMove ? _moveHistoryP1 : _moveHistoryP2;
            if (mmap.ContainsEdge(_clickedVertex, targetVertex))
            {
                hist.AddVerticesAndEdge(new Edge<Vertex>(_clickedVertex, targetVertex));
                breakContrEdge(_clickedVertex, targetVertex);
            }

            _clickedVertex = null;
            _availableVertices.Clear();
            increaseMoveCount();
        }

        private Edge<Vertex> getEdgeBetween(bool isFirst, Vertex source, Vertex target)
        {
            Vertex p1, p2;
            Vertex first = source;
            Vertex last = target;
            if (target < source)
            {
                first = target;
                last = source;
            }

            var f = isFirst ? 1 : 0;
            var nf = 1 - f;
            if (isFirst ? Vertex.OnHorizontal(first, last) : Vertex.OnVertical(first, last))
            {
                p2 = last;
                p1 = new Vertex(p2.Item1 - f, p2.Item2 - nf);
            }
            else
            {
                p1 = first;
                p2 = new Vertex(p1.Item1 + nf, p1.Item2 + f);
            }

            return new Edge<Vertex>(p1, p2);
        }

        private void breakContrEdge(Vertex source, Vertex target)
        {
            var mmap = IsFirstPlayerMove ? _moveMapP2 : _moveMapP1;
            var e = getEdgeBetween(IsFirstPlayerMove, source, target);
            mmap.RemoveEdgeIf(edge => 
                edge.Source.Equals(e.Source) && edge.Target.Equals(e.Target) ||
                edge.Source.Equals(e.Target) && edge.Target.Equals(e.Source));
        }

        
        
    public class AIPlayer
    {
        private LinesGame _game;
        Thread aiThread;
        private VertexList<Vertex> source_points;
        private VertexList<Vertex> target_points;

        private Func<Edge<Vertex>, double> edgeCost = e => 1;


        public AIPlayer(LinesGame game)
        {
            _game = game;
            source_points = new VertexList<Vertex>();
            target_points = new VertexList<Vertex>();
            foreach (var r in Enumerable.Range(0, Config.POINT_NUMBER_0))
            {
                source_points.Add(new Vertex(r, 0));
                target_points.Add(new Vertex(r, Config.POINT_NUMBER_0));
            }

            aiThread = new Thread(run);
            aiThread.Name = "AIThread";
            aiThread.Start();
        }

        public void run()
        {
            while (aiThread.IsAlive)
            {
                Thread.Sleep(1000);
                if (!_game.IsFirstPlayerMove)
                {
                    DoMove();
                }
            }
        }

//            private void ChooseStrategy()
//            {
//                var rand = new Random(Seed: 12);
//                var source = source_points[rand.Next(0, source_points.Count)];
//                List<IEnumerable<Edge<Vertex>>> paths = new List<IEnumerable<Edge<Vertex>>>();
//                var tryGetPath = _game._moveMapP2.ShortestPathsDijkstra(edgeCost, source);
//
//                foreach (var target in target_points)
//                {
//                    IEnumerable<Edge<Vertex>> path;
//                    if (tryGetPath(target, out path))
//                        paths.Add(path);
//                }
//
//                var minpath = paths[0];
//                foreach (var path in paths.Where(path => path.Count() < minpath.Count()))
//                {
//                    minpath = path;
//                }
//
//                strategy = minpath.ToList();
//            }
//
//            private bool isStrategyValid()
//            {
//                IEnumerable<Edge<Vertex>> path;
//                var tryGetPath = _game._moveMapP2.ShortestPathsDijkstra(edgeCost, strategy[0].Source);
//                if (tryGetPath(strategy.Last().Target, out path))
//                {
//                    var lPath = path.ToList();
//                    if (lPath.Count() != strategy.Count()) return false;
//                    for (var i = 0; i < lPath.Count(); ++i)
//                    {
//                        if ((!lPath[i].Source.Equals(strategy[i].Source)) ||
//                            (!lPath[i].Target.Equals(strategy[i].Target)))
//                            return false;
//                    }
//
//                    return true;
//                }
//
//                return false;
//            }

        private bool isPathFree(Vertex source)
        {
            IEnumerable<Edge<Vertex>> path;
            var tryGetPath = _game._moveMapP2.ShortestPathsDijkstra(edgeCost, source);
            if (tryGetPath(new Vertex(source.Item1, Config.POINT_NUMBER_1 - 1), out path))
            {
                if (path.Count() == Config.POINT_NUMBER_1 - 1) return true;
            }

            return false;
        }

        private Vertex getRandomVertex(int col)
        {
            Vertex vertex;
            var i = new Random().Next(-1, Config.POINT_NUMBER_0 - 1);
            do
            {
                i = (i < Config.POINT_NUMBER_0 ? ++i : 0);
                vertex = new Vertex(i, col);
            } while (isPathFree(vertex));

            return vertex;
        }

        public void DoMove()
        {
            Vertex source, target;
            if (_game.MoveCount < 2)
            {
                source = getRandomVertex(0);
                target = new Vertex(source.Item1, source.Item2 + 1);
            }
            else if (_game.MoveCount < 4)
            {
                target = getRandomVertex(Config.POINT_NUMBER_1 - 1);
                source = new Vertex(target.Item1, target.Item2 - 1);
            }
            else
            {
                (source, target) = blockOpponent();
            }

            _game.clickHandler(source);
            _game.clickHandler(target);


//                if (!isStrategyValid()) ChooseStrategy();
//                if (_game._availableVertices.Contains(strategy[0].Target))
//                {
//                    _game.clickHandler(strategy[0].Target);
//                    source_points.Remove(strategy[0].Source);
//                }
//                else
//                {
//                     source_points.Remove()
//                }
//                _game.clickHandler(strategy[0].Target);
//                source_points.Remove(strategy[0].Source);
//                source_points.Add(strategy[0].Target);
//                strategy.RemoveAt(0);
        }

        private (Vertex, Vertex) blockOpponent()
        {
//            var opmove = _game._moveHistoryP1.Edges.Last();
//            if (Vertex.OnHorizontal(opmove.Source, opmove.Target))
//            {
//                if (opmove.Source < opmove.Target)
//            } TODO 
        }
    }
}

}