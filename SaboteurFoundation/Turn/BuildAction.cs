using SaboteurFoundation.Cards;

namespace SaboteurFoundation.Turn
{
    public class BuildAction : TurnAction
    {
        public int X { get; }
        public int Y { get; }

        // ReSharper disable once SuggestBaseTypeForParameter
        public BuildAction(TunnelCard card, int x, int y) : base(card)
        {
            X = x;
            Y = y;
        }
    }
}
