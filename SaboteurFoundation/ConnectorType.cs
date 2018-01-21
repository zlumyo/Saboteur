namespace SaboteurFoundation
{
    /// <summary>
    /// Тип прохода в туннеле.
    /// </summary>
    public enum ConnectorType
    {
        Up, Right, Down, Left
    }
    
    /// <summary>
    /// Расширения для типов прохода.
    /// </summary>
    public static class ConnectorTypeExtensions {

        /// <summary>
        /// Трансформация типа прохода на противоположный.
        /// </summary>
        /// <param name="cType">Тип прохода.</param>
        /// <returns>Инвертирвоанный тип прохода.</returns>
        public static ConnectorType Flip(this ConnectorType cType) {
            switch (cType)
            {
                case ConnectorType.Down:
                    return ConnectorType.Up;
                case ConnectorType.Left:
                    return ConnectorType.Right;
                case ConnectorType.Right:
                    return ConnectorType.Left;
                case ConnectorType.Up:
                    return ConnectorType.Down;
                default:
                    return cType;
            }
        }
        
        /// <summary>
        /// Получить дельту координаты X по типу хода.
        /// </summary>
        /// <param name="type">Тип хода.</param>
        /// <returns>Смещение координаты.</returns>
        public static int ToDeltaX(this ConnectorType type)
        {
            switch (type)
            {
                case ConnectorType.Down:
                    return 0;
                case ConnectorType.Up:
                    return 0;
                case ConnectorType.Left:
                    return -1;
                case ConnectorType.Right:
                    return 1;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Получить дельту координаты Y по типу хода.
        /// </summary>
        /// <param name="type">Тип хода.</param>
        /// <returns>Смещение координаты.</returns>
        public static int ToDeltaY(this ConnectorType type)
        {
            switch (type)
            {
                case ConnectorType.Down:
                    return -1;
                case ConnectorType.Up:
                    return 1;
                case ConnectorType.Left:
                    return 0;
                case ConnectorType.Right:
                    return 0;
                default:
                    return 0;
            }
        }
    }
}
