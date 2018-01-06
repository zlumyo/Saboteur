namespace SaboteurFoundation.Cards
{
    public class CollapseCard : AffectFieldCard
    {
        public override bool Equals(Card other)
        {
            return other is CollapseCard;
        }
    }
}
