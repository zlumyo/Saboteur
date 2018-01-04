namespace SaboteurFoundation
{
    public class Connector
    {
        ConnectorType Type { get; }
        GameCell Next { get; set; }

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
            return obj is Connector && (obj as Connector).Type == Type;
        }
    }
}