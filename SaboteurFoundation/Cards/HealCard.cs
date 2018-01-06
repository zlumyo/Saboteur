namespace SaboteurFoundation.Cards
{
    public class HealCard : AffectPlayerCard
    {
        protected HealCard(Effect effect)
        {
            Heal = effect;
        }

        public static HealCard FromEffect(Effect effect)
        {
            return new HealCard(effect);
        }

        public override bool Equals(Card other)
        {
            return other is HealCard hc && this.Heal == hc.Heal;
        }

        public Effect Heal { get; }
    }
}
