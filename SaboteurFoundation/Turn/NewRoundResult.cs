namespace SaboteurFoundation.Turn
{
    /// <summary>
    /// Новый раунд по завершении хода.
    /// </summary>
    public class NewRoundResult : TurnResult
    {
        /// <summary>
        /// Первый игрок, совершающий ход.
        /// </summary>
        public Player FristPlayer { get; }

        /// <summary>
        /// Новый раунд с указанным первым игроком.
        /// </summary>
        /// <param name="first">Игрок, делающий первый ходв новом раунде.</param>
        public NewRoundResult(Player first)
        {
            FristPlayer = first;
        }
    }
}
