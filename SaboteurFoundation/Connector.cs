namespace SaboteurFoundation
{
    public class Connector
    {
        public ConnectorType Type { get; }
        public GameCell Next { get; set; }

        public Connector(ConnectorType type)
        {
            Type = type;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is Connector c && c.Type == Type;
        }

        public static int ConnectorTypeToDeltaX(ConnectorType type)
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

        public static int ConnectorTypeToDeltaY(ConnectorType type)
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

        internal static ConnectorType FlipConnectorType(ConnectorType cType)
        {
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
    }
}