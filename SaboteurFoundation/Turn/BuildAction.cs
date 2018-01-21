using SaboteurFoundation.Cards;

namespace SaboteurFoundation.Turn
{
    /// <summary>
    /// Ход "постройка туннеля".
    /// </summary>
    public class BuildAction : TurnAction
    {
        /// <summary>
        /// X-координта на игровом поле, где будет произведена постройка.
        /// </summary>
        public int X { get; }
        /// <summary>
        /// Y-координта на игровом поле, где будет произведена постройка.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Построить туннель используя заданную карту по указанным координатам.
        /// </summary>
        /// <param name="card">Карта туннеля для постройки.</param>
        /// <param name="x">X-координта на игровом поле.</param>
        /// <param name="y">Y-координта на игровом поле.</param>
        public BuildAction(TunnelCard card, int x, int y) : base(card)
        {
            X = x;
            Y = y;
        }
    }
}
