namespace SaboteurFoundation.Turn
{
    public class PlayDebufAction : TurnAction
    {
        public Player PlayerToDebuf { get; }

        public PlayDebufAction(Card card, Player player) : base(card)
        {
            PlayerToDebuf = player;
        }
    }
}
