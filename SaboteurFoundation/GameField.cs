using System;
using System.Collections.Generic;
using System.Linq;

namespace SaboteurFoundation
{
    public class GameField
    {
        public GameCell Start { get; }
        public Dictionary<EndVariant, GameCell> Ends { get; }

        public GameField(EndVariant endGold)
        {
            var allConnectorTypes = Enum.GetValues(typeof(ConnectorType)).Cast<ConnectorType>();
            Start = new GameCell(CellType.START, allConnectorTypes.Select(x => new Connector(x)).ToHashSet());

            Ends = new Dictionary<EndVariant, GameCell>(3) {
                {
                    EndVariant.LEFT,
                    new GameCell(
                        EndVariant.LEFT == endGold ? CellType.GOLD : CellType.FAKE,
                        new HashSet<Connector>(2) { new Connector(ConnectorType.RIGHT), new Connector(ConnectorType.DOWN) }
                    )
                },
                {
                    EndVariant.CENTER,
                    new GameCell(
                        EndVariant.CENTER == endGold ? CellType.GOLD : CellType.FAKE,
                        allConnectorTypes.Select(x => new Connector(x)).ToHashSet()
                    )
                },
                {
                    EndVariant.RIGHT,
                    new GameCell(
                        EndVariant.RIGHT == endGold ? CellType.GOLD : CellType.FAKE,
                        new HashSet<Connector>(2) { new Connector(ConnectorType.LEFT), new Connector(ConnectorType.DOWN) }
                    )
                }
            };
        }
    }
}