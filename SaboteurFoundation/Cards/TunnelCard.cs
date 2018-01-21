using System;
using System.Collections.Generic;

namespace SaboteurFoundation.Cards
{
    /// <summary>
    /// Карта постройки туннеля.
    /// </summary>
    public class TunnelCard : Card
    {
        private TunnelCard(HashSet<ConnectorType> outs, bool isDeadlock)
        {
            Outs = outs;
            IsDeadlock = isDeadlock;
        }

        /// <summary>
        /// Создаёт карту постройки туннеля с заданными параметрами.
        /// </summary>
        /// <param name="up">Будет ли верхний проход?</param>
        /// <param name="right">Будет ли правый проход?</param>
        /// <param name="down">Будет ли нижний проход?</param>
        /// <param name="left">Будет ли левый проход?</param>
        /// <param name="isDeadLock">Это будет тупик?</param>
        /// <returns>Новая карта постройки туннеля.</returns>
        public static TunnelCard FromOuts(bool up = false, bool right = false, bool down = false, bool left = false, bool isDeadLock = false)
        {
            var set = new HashSet<ConnectorType>(4);
            if (up) set.Add(ConnectorType.Up);
            if (right) set.Add(ConnectorType.Right);
            if (down) set.Add(ConnectorType.Down);
            if (left) set.Add(ConnectorType.Left);

            if (set.Count == 0) throw new ArgumentOutOfRangeException("At least one must be true.");
            return new TunnelCard(set, isDeadLock);
        }

        public override bool Equals(Card other)
        {
            return other is TunnelCard tc && this.IsDeadlock == tc.IsDeadlock && this.Outs.SetEquals(tc.Outs);
        }

        /// <summary>
        /// Проходы.
        /// </summary>
        public HashSet<ConnectorType> Outs { get; }
        /// <summary>
        /// Флаг тупика.
        /// </summary>
        public bool IsDeadlock { get; }
    }
}