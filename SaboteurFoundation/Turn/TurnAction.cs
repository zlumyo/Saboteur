namespace SaboteurFoundation.Turn
{
    public abstract class TurnAction
    {
        public Card CardToAct { get; }

        protected TurnAction(Card card)
        {
            CardToAct = card;
        }
    }
}
