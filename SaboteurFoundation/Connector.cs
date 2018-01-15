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
                case ConnectorType.DOWN:
                    return 0;
                case ConnectorType.UP:
                    return 0;
                case ConnectorType.LEFT:
                    return -1;
                case ConnectorType.RIGHT:
                    return 1;
                default:
                    return 0;
            }
        }

        public static int ConnectorTypeToDeltaY(ConnectorType type)
        {
            switch (type)
            {
                case ConnectorType.DOWN:
                    return -1;
                case ConnectorType.UP:
                    return 1;
                case ConnectorType.LEFT:
                    return 0;
                case ConnectorType.RIGHT:
                    return 0;
                default:
                    return 0;
            }
        }

        public static ConnectorType FlipConnectorType(ConnectorType cType)
        {
            switch (cType)
            {
                case ConnectorType.DOWN:
                    return ConnectorType.UP;
                case ConnectorType.LEFT:
                    return ConnectorType.RIGHT;
                case ConnectorType.RIGHT:
                    return ConnectorType.LEFT;
                case ConnectorType.UP:
                    return ConnectorType.DOWN;
                default:
                    return cType;
            }
        }
    }
}