namespace SaboteurFoundation.Turn
{
    /// <summary>
    /// Абстрактное дествие во время хода игрока.
    /// </summary>
    public abstract class TurnAction
    {
        /// <summary>
        /// Карта, задействованная в данном ходе.
        /// </summary>
        public Card CardToAct { get; }

        protected TurnAction(Card card)
        {
            CardToAct = card;
        }
    }
}
