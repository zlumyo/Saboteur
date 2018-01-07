using System;

namespace SaboteurFoundation.Cards
{
    public class HealAlternativeCard : AffectPlayerCard
    {
        private HealAlternativeCard(Effect effect1, Effect effect2)
        {
            HealAlternative1 = effect1;
            HealAlternative2 = effect2;
        }

        public static HealAlternativeCard FromEffect(Effect effect1, Effect effect2)
        {
            if (effect1 == effect2) throw new ArgumentException("Effects must be different.");

            return new HealAlternativeCard(effect1, effect2);
        }

        public override bool Equals(Card other)
        {
            return other is HealAlternativeCard hac &&
                ((this.HealAlternative1 == hac.HealAlternative1 && this.HealAlternative2 == hac.HealAlternative2) ||
                (this.HealAlternative1 == hac.HealAlternative2 && this.HealAlternative2 == hac.HealAlternative1));
        }

        public Effect HealAlternative1 { get; }
        public Effect HealAlternative2 { get; }
    }
}
