using SaboteurFoundation.Cards;

namespace SaboteurFoundation.Turn
{
    public class PlayInvestigateAction : TurnAction
    {
        public EndVariant Variant { get; }

        public PlayInvestigateAction(InvestigateCard card, EndVariant endVariant) : base(card)
        {
            Variant = endVariant;
        }
    }
}
