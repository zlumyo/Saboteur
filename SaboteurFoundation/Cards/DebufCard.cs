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

        public override bool Equals(Card other)
        {
            return other is DebufCard dc && this.Debuf == dc.Debuf;
        }

        Effect Debuf { get; }
    }
}
