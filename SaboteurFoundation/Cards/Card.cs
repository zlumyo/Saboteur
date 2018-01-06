using System;

namespace SaboteurFoundation
{
    public abstract class Card : IEquatable<Card>
    {
        public abstract bool Equals(Card other);
    }
}
