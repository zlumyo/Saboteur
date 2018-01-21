namespace SaboteurFoundation.Turn
{
    /// <summary>
    /// Пропуск хода.
    /// </summary>
    public class SkipAction : TurnAction
    {
        /// <summary>
        /// Пропустить ход, сбросив указанную карту.
        /// </summary>
        /// <param name="card"></param>
        public SkipAction(Card card) : base(card)
        {
        }
    }
}
