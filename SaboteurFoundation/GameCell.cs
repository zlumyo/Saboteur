using System;
using System.Collections.Generic;
using System.Linq;

namespace SaboteurFoundation
{
    /// <summary>
    /// Ячейка на игровом поле.
    /// </summary>
    public class GameCell
    {
        /// <summary>
        /// Отображение проходов тоннеля на соседствующие ячейки.
        /// </summary>
        public Dictionary<ConnectorType, GameCell> Outs { get; }
        /// <summary>
        /// Тип ячейки.
        /// </summary>
        public CellType Type { get; }
        /// <summary>
        /// Флаг обрушенности туннеля.
        /// </summary>
        public bool HasCollapsed { get; internal set; }
        /// <summary>
        /// Флаг тупика.
        /// </summary>
        public bool IsDeadlock { get; }
        /// <summary>
        /// Координата ячейки по оси X.
        /// </summary>
        public int X { get; }
        /// <summary>
        /// Координата ячейки по оси Y.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Создать ячейку указанного типа с заданными координатам.
        /// </summary>
        /// <param name="type">Тип ячейки.</param>
        /// <param name="x">X-координата на игровом поле.</param>
        /// <param name="y">Y-координата на игровом поле.</param>
        /// <param name="outs">Список проходов.</param>
        /// <param name="isDeadlock">Флаг тупика.</param>
        public GameCell(CellType type, int x, int y, ICollection<ConnectorType> outs, bool isDeadlock)
        {
            if (outs.Count == 0) throw new ArgumentException("There must be at least one connector.");

            X = x;
            Y = y;
            IsDeadlock = isDeadlock;
            HasCollapsed = false;
            Type = type;
            Outs = outs.ToDictionary(o => o, o => default(GameCell));
        }
    }
}