using SaboteurFoundation.Cards;

namespace SaboteurFoundation.Turn
{
    /// <summary>
    /// Ход "починить один из двух инструментов игрока".
    /// </summary>
    public class HealAlternativeAction : TurnAction
    {
        /// <summary>
        /// Игрок, которому будет оказана помощь.
        /// </summary>
        public Player PlayerToHeal { get; }

        /// <summary>
        /// Починить один из двух указанных инструментов у заданного игрока.
        /// </summary>
        /// <param name="card">Карта альтернативной починки.</param>
        /// <param name="player">Игрок со сломанным инструментом.</param>
        public HealAlternativeAction(HealAlternativeCard card, Player player) : base(card)
        {
            PlayerToHeal = player;
        }
    }
}
