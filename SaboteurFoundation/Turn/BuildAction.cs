using SaboteurFoundation.Cards;

namespace SaboteurFoundation.Turn
{
    public class BuildAction : TurnAction
    {
        public GameCell NearCell { get; }
        public ConnectorType SideOfNearCard { get; }

        public BuildAction(TunnelCard card, GameCell nearCell, ConnectorType sideOfNearCard) : base(card)
        {
            NearCard = nearCell;
            SideOfNearCard = sideOfNearCard;
        }
    }
}
