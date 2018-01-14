using SaboteurFoundation.Cards;

namespace SaboteurFoundation.Turn
{
    public class CollapseAction : TurnAction
    {
        public int X { get; }
        public int Y { get; }

        public CollapseAction(CollapseCard card, int xNear, int yNear) : base(card)
        {
            X = xNear;
            Y = yNear;
        }
    }
}
