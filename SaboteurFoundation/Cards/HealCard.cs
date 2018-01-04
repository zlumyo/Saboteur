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

        public Effect Heal { get; }
    }
}
