using System;
using System.Collections.Generic;
using System.Linq;

namespace SaboteurFoundation
{
    public class GameField
    {
        public GameCell Start { get; }
        public Dictionary<EndVariant, GameCell> Ends { get; }

        public static Dictionary<EndVariant, (int, int)> EndsCoordinates { get; } = new Dictionary<EndVariant, (int, int)>
        {
            { EndVariant.Left, (-2, 8) },
            { EndVariant.Center, (0, 8) },
            { EndVariant.Right, (2, 8) }
        };

        public GameField(EndVariant endGold)
        {
            var allConnectorTypes = Enum.GetValues(typeof(ConnectorType)).Cast<ConnectorType>().ToArray();
            Start = new GameCell(CellType.Start, allConnectorTypes.Select(x => new Connector(x)).ToHashSet(), false);

            Ends = new Dictionary<EndVariant, GameCell>(3) {
                {
                    EndVariant.Left,
                    new GameCell(
                        EndVariant.Left == endGold ? CellType.Gold : CellType.Fake,
                        new HashSet<Connector>(2) { new Connector(ConnectorType.Right), new Connector(ConnectorType.Down) },
                        false
                    )
                },
                {
                    EndVariant.Center,
                    new GameCell(
                        EndVariant.Center == endGold ? CellType.Gold : CellType.Fake,
                        allConnectorTypes.Select(x => new Connector(x)).ToHashSet(),
                        false
                    )
                },
                {
                    EndVariant.Right,
                    new GameCell(
                        EndVariant.Right == endGold ? CellType.Gold : CellType.Fake,
                        new HashSet<Connector>(2) { new Connector(ConnectorType.Left), new Connector(ConnectorType.Down) },
                        false
                    )
                }
            };
        }

        public bool CheckFinishReached(GameCell cell, int xCell, int yCell, out GameCell[] finishes)
        {
            finishes = cell.Outs.Select(_out => (xCell + Connector.ConnectorTypeToDeltaX(_out.Type), yCell + Connector.ConnectorTypeToDeltaY(_out.Type))).Where(pair => EndsCoordinates.ContainsValue(pair))
                .Select(pair => Ends[EndsCoordinates.First(p => p.Value.Item1 == pair.Item1 && p.Value.Item2 == pair.Item2).Key]).ToArray();
            return finishes.Length != 0;
        }
    }
}