namespace SaboteurFoundation.Turn
{
    /// <summary>
    /// Окончание игры по завершении хода.
    /// </summary>
    public class EndGameResult : TurnResult
    {
        /// <summary>
        /// Таблица победителей.
        /// </summary>
        public Player[] Winners { get; }

        /// <summary>
        /// Конец игры с таблицей победителей.
        /// </summary>
        /// <param name="winners">Игроки отсоритрвоанные по убывающему количеству золота.</param>
        public EndGameResult(Player[] winners)
        {
            Winners = winners;
        }
    }
}
