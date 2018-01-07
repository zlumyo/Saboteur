namespace SaboteurFoundation.Turn
{
    public class PlayInvestigateAction : TurnAction
    {
        public EndVariant Variant { get; }

        public PlayInvestigateAction(Card card, EndVariant endVariant) : base(card)
        {
            Variant = endVariant;
        }
    }
}
