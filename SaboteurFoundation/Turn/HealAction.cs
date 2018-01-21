using SaboteurFoundation.Cards;

namespace SaboteurFoundation.Turn
{
    /// <summary>
    /// Ход "починить инструмент игрока".
    /// </summary>
    public class HealAction : TurnAction
    {
        /// <summary>
        /// Игрок, которому будет оказана помощь.
        /// </summary>
        public Player PlayerToHeal { get; }

        /// <summary>
        /// Починить указанный инструмент у заданного игрока.
        /// </summary>
        /// <param name="card">Карта починки.</param>
        /// <param name="player">Игрок со сломанным инструментом.</param>
        public HealAction(HealCard card, Player player) : base(card)
        {
            PlayerToHeal = player;
        }
    }
}
