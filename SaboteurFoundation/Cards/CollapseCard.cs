namespace SaboteurFoundation.Cards
{
    /// <summary>
    /// Карта обрушения туннеля.
    /// </summary>
    public class CollapseCard : AffectFieldCard
    {
        public override bool Equals(Card other)
        {
            return other is CollapseCard;
        }
    }
}
