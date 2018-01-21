using SaboteurFoundation.Cards;

namespace SaboteurFoundation.Turn
{
    /// <summary>
    /// Ход "подсмотреть золотую жилу".
    /// </summary>
    public class InvestigateAction : TurnAction
    {
        /// <summary>
        /// Золотая жила, которая будет подсмотрена.
        /// </summary>
        public EndVariant Variant { get; }

        /// <summary>
        /// Подсмотреть указанную золотую жилу.
        /// </summary>
        /// <param name="card">Карта подсмотра золотой жилы.</param>
        /// <param name="endVariant">Вариант золотой жилы.</param>
        public InvestigateAction(InvestigateCard card, EndVariant endVariant) : base(card)
        {
            Variant = endVariant;
        }
    }
}
