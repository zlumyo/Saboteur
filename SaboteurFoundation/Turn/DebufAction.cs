using SaboteurFoundation.Cards;

namespace SaboteurFoundation.Turn
{
    /// <summary>
    /// Ход "сломать инструмент игрока".
    /// </summary>
    public class DebufAction : TurnAction
    {
        /// <summary>
        /// Игрок, которому будет сломан инструмент.
        /// </summary>
        public Player PlayerToDebuf { get; }

        /// <summary>
        /// Поломать указанный инструмент у заданного игрока.
        /// </summary>
        /// <param name="card">Карта поломки.</param>
        /// <param name="player">Игрок со исправным инструментом.</param>
        public DebufAction(DebufCard card, Player player) : base(card)
        {
            PlayerToDebuf = player;
        }
    }
}
