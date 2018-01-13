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
    }
}