using System;
using System.Collections.Generic;

namespace SaboteurFoundation
{
    public class GameCell
    {
        public HashSet<Connector> Outs { get; }
        public CellType Type { get; }
        public bool HasCollapsed { get; internal set; }
        public bool IsDeadlock { get; }

        public GameCell(CellType type, HashSet<Connector> outs, bool isDeadlock)
        {
            if (outs.Count == 0) throw new ArgumentException("There must be at least one connector.");

            IsDeadlock = isDeadlock;
            HasCollapsed = false;
            Type = type;
            Outs = outs;
        }
    }
}