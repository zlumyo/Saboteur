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
            { EndVariant.LEFT, (-2, 8) },
            { EndVariant.CENTER, (0, 8) },
            { EndVariant.RIGHT, (2, 8) }
        };

        public GameField(EndVariant endGold)
        {
            var allConnectorTypes = Enum.GetValues(typeof(ConnectorType)).Cast<ConnectorType>();
            Start = new GameCell(CellType.START, allConnectorTypes.Select(x => new Connector(x)).ToHashSet(), false);

            Ends = new Dictionary<EndVariant, GameCell>(3) {
                {
                    EndVariant.LEFT,
                    new GameCell(
                        EndVariant.LEFT == endGold ? CellType.GOLD : CellType.FAKE,
                        new HashSet<Connector>(2) { new Connector(ConnectorType.RIGHT), new Connector(ConnectorType.DOWN) },
                        false
                    )
                },
                {
                    EndVariant.CENTER,
                    new GameCell(
                        EndVariant.CENTER == endGold ? CellType.GOLD : CellType.FAKE,
                        allConnectorTypes.Select(x => new Connector(x)).ToHashSet(),
                        false
                    )
                },
                {
                    EndVariant.RIGHT,
                    new GameCell(
                        EndVariant.RIGHT == endGold ? CellType.GOLD : CellType.FAKE,
                        new HashSet<Connector>(2) { new Connector(ConnectorType.LEFT), new Connector(ConnectorType.DOWN) },
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