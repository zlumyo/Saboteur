namespace SaboteurFoundation.Turn
{
    /// <summary>
    /// Новый ход после завершения текущего.
    /// </summary>
    public class NewTurnResult : TurnResult
    {
        /// <summary>
        /// Следующий ходящий игрок.
        /// </summary>
        public Player NextPlayer { get; }

        /// <summary>
        /// Новый ход указанным игроком.
        /// </summary>
        /// <param name="next">Игрок, который будет ходить следующим.</param>
        public NewTurnResult(Player next)
        {
            NextPlayer = next;
        }
    }
}
