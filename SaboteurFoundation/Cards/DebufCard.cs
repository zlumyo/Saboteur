namespace SaboteurFoundation
{
    public class DebufCard : AffectPlayerCard
    {
        private DebufCard(Effect effect)
        {
            Debuf = effect;
        }

        public static DebufCard FromEffect(Effect effect)
        {
            return new DebufCard(effect);
        }

        Effect Debuf { get; }
    }
}
