using System;
using System.Collections.Generic;
using System.Linq;

namespace SaboteurFoundation
{
    public class GameField
    {
        public GameCell Start { get; }
        public Dictionary<EndVariant, GameCell> Ends { get; }

        public static Dictionary<EndVariant, (int, int)> EndsCoordinates { get; } =
            new Dictionary<EndVariant, (int, int)>
        {
            { EndVariant.Left, (-2, 8) },
            { EndVariant.Center, (0, 8) },
            { EndVariant.Right, (2, 8) }
        };

        public GameField(EndVariant endGold)
        {
            var allConnectorTypes = Enum.GetValues(typeof(ConnectorType)).Cast<ConnectorType>().ToArray();
            Start = new GameCell(CellType.Start, 0, 0, allConnectorTypes, false);

            Ends = new Dictionary<EndVariant, GameCell>(3) {
                {
                    EndVariant.Left,
                    new GameCell(
                        EndVariant.Left == endGold ? CellType.Gold : CellType.Fake,
                        EndsCoordinates[EndVariant.Left].Item1, EndsCoordinates[EndVariant.Left].Item2,
                        new[] { ConnectorType.Right, ConnectorType.Down },
                        false
                    )
                },
                {
                    EndVariant.Center,
                    new GameCell(
                        EndVariant.Center == endGold ? CellType.Gold : CellType.Fake,
                        EndsCoordinates[EndVariant.Center].Item1, EndsCoordinates[EndVariant.Center].Item2,
                        allConnectorTypes,
                        false
                    )
                },
                {
                    EndVariant.Right,
                    new GameCell(
                        EndVariant.Right == endGold ? CellType.Gold : CellType.Fake,
                        EndsCoordinates[EndVariant.Right].Item1, EndsCoordinates[EndVariant.Right].Item2,
                        new[] { ConnectorType.Left, ConnectorType.Down },
                        false
                    )
                }
            };
        }

        public bool CheckFinishReached(GameCell cell, int xCell, int yCell, out (GameCell, int, int)[] finishes)
        {
            finishes = cell.Outs.Select(pair => (xCell + pair.Key.ToDeltaX(), yCell + pair.Key.ToDeltaY()))
                .Where(pair => EndsCoordinates.ContainsValue(pair))
                .Select(pair =>
                    (Ends[EndsCoordinates.First(p => p.Value.Item1 == pair.Item1 && p.Value.Item2 == pair.Item2).Key],
                    pair.Item1, pair.Item2))
                .ToArray();
            return finishes.Length != 0;
        }

        internal GameCell Scan(int xTarget, int yTarget)
        {
            var watched = new HashSet<(int, int)>();
            return ScanHelper(Start, 0, 0, new List<(int,int)> { (xTarget, yTarget) }, watched).FirstOrDefault();
        }
        
        private static GameCell[] ScanHelper(GameCell current, int xCurrent, int yCurrent,
            ICollection<(int, int)> targets, ISet<(int, int)> watched)
        {
            if (current.HasCollapsed) return new GameCell[]{};

            if (targets.Contains((xCurrent, yCurrent)))
            {
                targets.Remove((xCurrent, yCurrent));
                watched.Add((xCurrent, yCurrent));
                return new[]{current};
            }
                
            watched.Add((xCurrent, yCurrent));
            if (current.IsDeadlock)
                return new GameCell[]{};

            return current.Outs
                // фильтруем соседей, которые уже просмотрены 
                .Where(pair => pair.Value != null &&
                               !watched.Contains((xCurrent + pair.Key.ToDeltaX(), yCurrent + pair.Key.ToDeltaY())))
                // сканируем соседей
                .Select(pair => ScanHelper(pair.Value, xCurrent + pair.Key.ToDeltaX(), yCurrent + pair.Key.ToDeltaY(),
                    targets, watched))
                // фильтруем пустые результаты сканов
                .Where(result => result.Length != 0).SelectMany(i => i).ToArray();
        }

        internal GameCell PutNewTunnel(int x, int y, HashSet<ConnectorType> outs, bool isDeadlock)
        {
            var newCell = new GameCell(CellType.Tunnel, x, y, outs, isDeadlock);
            
            foreach (var neighbor in FindNighbors(x, y))
            {
                ConnectorType fromNeighborToNew;
                
                if (neighbor.X == x)
                {
                    fromNeighborToNew = neighbor.Y < y ? ConnectorType.Up : ConnectorType.Down;
                }
                else
                {
                    fromNeighborToNew = neighbor.X < x ? ConnectorType.Right : ConnectorType.Left;
                }

                var fromNewToNeighbor = fromNeighborToNew.Flip();
                
                // от соседа к новому тоннелю
                neighbor.Outs[fromNeighborToNew] = newCell;
                neighbor.Outs[fromNeighborToNew].Outs[fromNewToNeighbor] = neighbor; // обратная связь
            }

            return newCell;
        }

        private IEnumerable<GameCell> FindNighbors(int x, int y)
        {
            var watched = new HashSet<(int, int)>();
            var targets = new List<(int,int)> { (x+1, y), (x-1, y), (x, y+1), (x, y-1) };
            return ScanHelper(Start, 0, 0, targets, watched);
        }
        
        internal static void ConnectToFinish(GameCell pretender, int pX, int pY, GameCell finish, int fX)
        {
            ConnectorType fromPretenderToFinish;
            
            switch (pY)
            {
                case 7:
                    fromPretenderToFinish = ConnectorType.Up;
                    break;
                case 9:
                    fromPretenderToFinish = ConnectorType.Down;
                    break;
                default:
                    if (pX == -1 || pX == 3)
                        fromPretenderToFinish = fX != 0 ? ConnectorType.Left : ConnectorType.Right;
                    else
                        fromPretenderToFinish = fX != 0 ? ConnectorType.Right : ConnectorType.Left;

                    break;
            }
            
            var fromFinishToPretender = fromPretenderToFinish.Flip();
            
            if (pretender.Outs.TryAdd(fromPretenderToFinish, finish))
                finish.Outs[fromFinishToPretender] = pretender;
        }
    }
}