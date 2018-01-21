using SaboteurFoundation.Cards;

namespace SaboteurFoundation.Turn
{
    /// <summary>
    /// Ход "обрушить туннель".
    /// </summary>
    public class CollapseAction : TurnAction
    {
        /// <summary>
        /// X-координата на игровом поле, где убдет обрушен туннель.
        /// </summary>
        public int X { get; }
        /// <summary>
        /// Y-координата на игровом поле, где убдет обрушен туннель.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Обрушить туннель по указанным координатам.
        /// </summary>
        /// <param name="card">Карта обрушения туннеля.</param>
        /// <param name="xNear">X-координата на игрвом поле.</param>
        /// <param name="yNear">Y-координата на игровом поле.</param>
        public CollapseAction(CollapseCard card, int xNear, int yNear) : base(card)
        {
            X = xNear;
            Y = yNear;
        }
    }
}
