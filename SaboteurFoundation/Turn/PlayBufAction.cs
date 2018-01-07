using SaboteurFoundation.Cards;

namespace SaboteurFoundation.Turn
{
    public class PlayBufAction : TurnAction
    {
        public Player PlayerToBuf { get; }

        public PlayBufAction(HealCard card, Player player) : base(card)
        {
            PlayerToBuf = player;
        }
    }
}
