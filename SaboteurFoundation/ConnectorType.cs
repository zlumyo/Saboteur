namespace SaboteurFoundation
{
    public enum ConnectorType
    {
        Up, Right, Down, Left
    }
    
    public static class ConnectorTypeExtensions {
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
