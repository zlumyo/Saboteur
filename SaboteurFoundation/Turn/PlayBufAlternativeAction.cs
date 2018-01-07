using SaboteurFoundation.Cards;

namespace SaboteurFoundation.Turn
{
    public class PlayBufAlternativeAction : TurnAction
    {
        public Player PlayerToBuf { get; }

        public PlayBufAlternativeAction(HealAlternativeCard card, Player player) : base(card)
        {
            PlayerToBuf = player;
        }
    }
}
