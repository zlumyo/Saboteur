using System;
using System.Collections.Generic;
using System.Linq;

namespace SaboteurFoundation
{
    public class GameCell
    {
        public Dictionary<ConnectorType, GameCell> Outs { get; }
        public CellType Type { get; }
        public bool HasCollapsed { get; internal set; }
        public bool IsDeadlock { get; }
        public int X { get; }
        public int Y { get; }

        public GameCell(CellType type, int x, int y, ICollection<ConnectorType> outs, bool isDeadlock)
        {
            if (outs.Count == 0) throw new ArgumentException("There must be at least one connector.");

            X = x;
            Y = y;
            IsDeadlock = isDeadlock;
            HasCollapsed = false;
            Type = type;
            Outs = outs.ToDictionary(o => o, o => default(GameCell));
        }
    }
}