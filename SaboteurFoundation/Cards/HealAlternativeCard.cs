using System;

namespace SaboteurFoundation.Cards
{
    /// <summary>
    /// Карта починки одного из двух инструментов.
    /// </summary>
    public class HealAlternativeCard : AffectPlayerCard
    {
        private HealAlternativeCard(Effect effect1, Effect effect2)
        {
            HealAlternative1 = effect1;
            HealAlternative2 = effect2;
        }

        /// <summary>
        /// Создаёт карту починки с двумя указанными альтернативами.
        /// </summary>
        /// <param name="effect1">Альтернатива починки №1.</param>
        /// <param name="effect2">Альтернатива починки №2.</param>
        /// <returns>Новая карта альтернативной починки.</returns>
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

        /// <summary>
        /// Инструмент, который может починить карта №1.
        /// </summary>
        public Effect HealAlternative1 { get; }
        /// <summary>
        /// Инструмент, который может починить карта №2.
        /// </summary>
        public Effect HealAlternative2 { get; }
    }
}
