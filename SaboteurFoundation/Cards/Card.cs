using System;

namespace SaboteurFoundation
{
    /// <summary>
    /// Абстрактная карта в колоде или руке.
    /// </summary>
    public abstract class Card : IEquatable<Card>
    {
        public abstract bool Equals(Card other);
    }
}
