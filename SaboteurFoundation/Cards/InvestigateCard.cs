namespace SaboteurFoundation.Cards
{
    public class InvestigateCard : AffectFieldCard
    {
        public override bool Equals(Card other)
        {
            return other is InvestigateCard;
        }
    }
}
