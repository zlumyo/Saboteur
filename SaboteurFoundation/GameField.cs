using System;
using System.Collections.Generic;
using System.Linq;

namespace SaboteurFoundation
{
    /// <summary>
    /// Игровое поле.
    /// </summary>
    public class GameField
    {
        /// <summary>
        /// Стартовая ячейка с координатой (0,0).
        /// </summary>
        public GameCell Start { get; }
        /// <summary>
        /// Золотые жилы.
        /// </summary>
        public Dictionary<EndVariant, GameCell> Ends { get; }

        /// <summary>
        /// Координаты золотых жил, используемые при создании поля.
        /// </summary>
        public static Dictionary<EndVariant, (int, int)> EndsCoordinates { get; } =
            new Dictionary<EndVariant, (int, int)>
        {
            { EndVariant.Left, (-2, 8) },
            { EndVariant.Center, (0, 8) },
            { EndVariant.Right, (2, 8) }
        };

        /// <summary>
        /// Создаёт поле с указанием места реального золота.
        /// </summary>
        /// <param name="endGold">Место реального золота.</param>
        public GameField(EndVariant endGold)
        {
            var allConnectorTypes = Enum.GetValues(typeof(ConnectorType)).Cast<ConnectorType>().ToArray();
            var allEndVariants = Enum.GetValues(typeof(EndVariant)).Cast<EndVariant>().ToArray();
            Start = new GameCell(CellType.Start, 0, 0, allConnectorTypes, false);

            Ends = allEndVariants.ToDictionary(ev => ev, ev =>            
                new GameCell(
                    ev == endGold ? CellType.Gold : CellType.Fake,
                    EndsCoordinates[ev].Item1, EndsCoordinates[ev].Item2,
                    allConnectorTypes,
                    false
                )
            );
        }

        internal bool CheckFinishReached(GameCell cell, out GameCell[] finishes)
        {
            finishes = cell.Outs
                .Select(pair => (cell.X + pair.Key.ToDeltaX(), cell.Y + pair.Key.ToDeltaY()))
                .Where(pair => EndsCoordinates.ContainsValue(pair))
                .Select(pair => Ends[EndsCoordinates.First(p => p.Value.Item1 == pair.Item1 && p.Value.Item2 == pair.Item2).Key])
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
                var result = new[] { current };

                if (current.IsDeadlock)
                    return result;
                else
                    return result.Concat(
                        current.Outs
                            // фильтруем соседей, которые уже просмотрены 
                            .Where(pair => pair.Value != null &&
                                           !watched.Contains((xCurrent + pair.Key.ToDeltaX(), yCurrent + pair.Key.ToDeltaY())))
                            // сканируем соседей
                            .Select(pair => ScanHelper(pair.Value, xCurrent + pair.Key.ToDeltaX(), yCurrent + pair.Key.ToDeltaY(),
                                targets, watched))
                            // фильтруем пустые результаты сканов
                            .Where(r => r.Length != 0).SelectMany(i => i)
                        ).ToArray();
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

        internal GameCell PutNewTunnel(int x, int y, ISet<ConnectorType> outs, bool isDeadlock)
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
                
                if (neighbor.Outs.ContainsKey(fromNeighborToNew))
                {                  
                    neighbor.Outs[fromNeighborToNew] = newCell; // от соседа к новому тоннелю
                    neighbor.Outs[fromNeighborToNew].Outs[fromNewToNeighbor] = neighbor; // обратная связь
                }
                
            }

            return newCell;
        }

        internal IEnumerable<GameCell> FindNighbors(int x, int y)
        {
            var watched = new HashSet<(int, int)>();
            var targets = new List<(int,int)> { (x+1, y), (x-1, y), (x, y+1), (x, y-1) };
            return ScanHelper(Start, 0, 0, targets, watched);
        }
        
        internal void ConnectFinish(GameCell finish)
        {           
            foreach (var neighbor in FindNighbors(finish.X, finish.Y))
            {
                ConnectorType fromPretenderToFinish;
                
                switch (neighbor.Y)
                {
                    case 7:
                        fromPretenderToFinish = ConnectorType.Up;
                        break;
                    case 9:
                        fromPretenderToFinish = ConnectorType.Down;
                        break;
                    default:
                        if (neighbor.X == -1 || neighbor.X == 3)
                            fromPretenderToFinish = finish.X != 0 ? ConnectorType.Left : ConnectorType.Right;
                        else
                            fromPretenderToFinish = finish.X != 0 ? ConnectorType.Right : ConnectorType.Left;

                        break;
                }
            
                var fromFinishToPretender = fromPretenderToFinish.Flip();
            
                if (neighbor.Outs.ContainsKey(fromPretenderToFinish))
                    finish.Outs[fromFinishToPretender] = neighbor;
            }         
        }
    }
}