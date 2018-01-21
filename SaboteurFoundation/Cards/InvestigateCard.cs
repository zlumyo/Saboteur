namespace SaboteurFoundation.Cards
{
    /// <summary>
    /// Карта подсмотра золотой жилы.
    /// </summary>
    public class InvestigateCard : AffectFieldCard
    {
        public override bool Equals(Card other)
        {
            return other is InvestigateCard;
        }
    }
}
