namespace SaboteurFoundation.Cards
{
    /// <summary>
    /// Карта поломки заданного инструмента.
    /// </summary>
    public class DebufCard : AffectPlayerCard
    {       
        private DebufCard(Effect effect)
        {
            Debuf = effect;
        }

        /// <summary>
        /// Создаёт карту поломки указанного инструмента.
        /// </summary>
        /// <param name="effect">Инструмент, который необходимо поломать.</param>
        /// <returns>Новая карта поломки.</returns>
        public static DebufCard FromEffect(Effect effect)
        {
            return new DebufCard(effect);
        }

        public override bool Equals(Card other)
        {
            return other is DebufCard dc && this.Debuf == dc.Debuf;
        }

        /// <summary>
        /// Инструмент, который поломает карта.
        /// </summary>
        public Effect Debuf { get; }
    }
}
