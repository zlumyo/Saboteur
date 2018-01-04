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

        Effect HealAlternative1 { get; }
        Effect HealAlternative2 { get; }
    }
}
