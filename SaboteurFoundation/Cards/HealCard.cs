namespace SaboteurFoundation.Cards
{
    /// <summary>
    /// Карта починки заданного инструмента.
    /// </summary>
    public class HealCard : AffectPlayerCard
    {
        private HealCard(Effect effect)
        {
            Heal = effect;
        }

        /// <summary>
        /// Создаёт карту починки указанного инструмента.
        /// </summary>
        /// <param name="effect">Инструмент, который необходимо починить.</param>
        /// <returns>Новая карта починки.</returns>
        public static HealCard FromEffect(Effect effect)
        {
            return new HealCard(effect);
        }

        public override bool Equals(Card other)
        {
            return other is HealCard hc && this.Heal == hc.Heal;
        }

        /// <summary>
        /// Инструмент, который починит карта.
        /// </summary>
        public Effect Heal { get; }
    }
}
