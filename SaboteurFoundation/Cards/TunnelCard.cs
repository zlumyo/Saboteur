using System;
using System.Collections.Generic;

namespace SaboteurFoundation.Cards
{
    public class TunnelCard : Card
    {
        private TunnelCard(HashSet<ConnectorType> outs, bool isDeadlock)
        {
            Outs = outs;
            IsDeadlock = isDeadlock;
        }

        public static TunnelCard FromOuts(bool up = false, bool right = false, bool down = false, bool left = false, bool isDeadLock = false)
        {
            var set = new HashSet<ConnectorType>(4);
            if (up) set.Add(ConnectorType.UP);
            if (right) set.Add(ConnectorType.RIGHT);
            if (down) set.Add(ConnectorType.DOWN);
            if (left) set.Add(ConnectorType.LEFT);

            if (set.Count == 0) throw new ArgumentOutOfRangeException("At least one must be true.");
            return new TunnelCard(set, isDeadLock);
        }

        public override bool Equals(Card other)
        {
            return other is TunnelCard tc && this.IsDeadlock == tc.IsDeadlock && this.Outs.SetEquals(tc.Outs);
        }

        public HashSet<ConnectorType> Outs { get; }
        public bool IsDeadlock { get; }
    }
}