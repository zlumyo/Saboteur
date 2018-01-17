using System.Linq;
using SaboteurFoundation;
using SaboteurFoundation.Cards;
using SaboteurFoundation.Turn;

namespace SaboteurTest
{
    public static class Utils
    {
        public static TurnResult BuildTunnelAt(SaboteurGame game, int x, int y, ConnectorType side, bool withOppositeSide = false)
        {
            return BuildTunnelAtBy(game, x, y, side, withOppositeSide);
        }
        
        public static TurnResult BuildTunnelAtBy(SaboteurGame game, int x, int y, ConnectorType side, bool withOppositeSide = false, Player builder = null)
        {          
            var flippedSide = side.Flip();
            // ReSharper disable once PossibleNullReferenceException
            while (builder != null ^ game.CurrentPlayer == builder ^ game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && !tc.IsDeadlock && tc.Outs.Contains(flippedSide) && (!withOppositeSide || tc.Outs.Contains(side))) == 0)
            {
                game.ExecuteTurn(new SkipAction(game.CurrentPlayer.Hand.First()));
            }

            var tunnelCard = game.CurrentPlayer.Hand.Find(c => c is TunnelCard tc && !tc.IsDeadlock && tc.Outs.Contains(flippedSide) && (!withOppositeSide || tc.Outs.Contains(side))) as TunnelCard;

            return game.ExecuteTurn(new BuildAction(tunnelCard, x, y, side));
        }
    }
}