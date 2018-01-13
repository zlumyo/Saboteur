using SaboteurFoundation.Cards;

namespace SaboteurFoundation.Turn
{
    public class BuildAction : TurnAction
    {
        public int XNear { get; }
        public int YNear { get; }
        public ConnectorType SideOfNearCard { get; }

        public BuildAction(TunnelCard card, int xNear, int yNear, ConnectorType sideOfNearCard) : base(card)
        {
            XNear = xNear;
            YNear = yNear;
            SideOfNearCard = sideOfNearCard;
        }
    }
}
