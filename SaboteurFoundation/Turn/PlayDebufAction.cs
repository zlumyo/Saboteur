using SaboteurFoundation.Cards;

namespace SaboteurFoundation.Turn
{
    public class PlayDebufAction : TurnAction
    {
        public Player PlayerToDebuf { get; }

        public PlayDebufAction(DebufCard card, Player player) : base(card)
        {
            PlayerToDebuf = player;
        }
    }
}
