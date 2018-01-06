using System;
using System.Collections.Generic;

namespace SaboteurFoundation
{
    public class GameCell
    {
        public HashSet<Connector> Outs { get; }
        public CellType Type { get; }

        public GameCell(CellType type, HashSet<Connector> outs)
        {
            if (outs.Count == 0) throw new ArgumentException("There must be at least one connector.");

            Type = type;
            Outs = outs;
        }
    }
}