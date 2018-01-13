using SaboteurFoundation.Cards;

namespace SaboteurFoundation.Turn
{
    public class BuildAction : TurnAction
    {
        public TunnelCard NearCard { get; }
        public ConnectorType SideOfNearCard { get; }

        public BuildAction(TunnelCard card, TunnelCard nearCard, ConnectorType sideOfNearCard) : base(card)
        {
            NearCard = nearCard;
            SideOfNearCard = sideOfNearCard;
        }
    }
}
