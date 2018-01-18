using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SaboteurFoundation;
using SaboteurFoundation.Cards;
using SaboteurFoundation.Turn;

namespace SaboteurTest
{
    [TestClass]
    public class SaboteurEndGameTest
    {
        private static readonly string[] MinPlayers = { "player1", "player2", "player3" };

        private SaboteurGame _game;

        [TestInitialize]
        public void TestInit()
        {
            _game = SaboteurGame.NewGame(false, false, MinPlayers);
        }
        
        [TestMethod]
        public void NextRoundTest()
        {
            var direction = _game.Field.Ends.First(end => end.Value.Type == CellType.Gold).Key;
            int xBase;
            switch (direction)
            {
                case EndVariant.Left:
                    Utils.BuildTunnelAt(_game, 0, 0, ConnectorType.Left, true);
                    
                    while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && !tc.IsDeadlock && tc.Outs.Contains(ConnectorType.Right) && tc.Outs.Contains(ConnectorType.Up)) == 0)
                    {
                        _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
                    }
                    var tunnelCardLeft = _game.CurrentPlayer.Hand
                            .Find(c => c is TunnelCard tc && !tc.IsDeadlock && tc.Outs.Contains(ConnectorType.Right) && tc.Outs.Contains(ConnectorType.Up))
                        as TunnelCard;
                    _game.ExecuteTurn(new BuildAction(tunnelCardLeft, -1, 0, ConnectorType.Left));
                    
                    xBase = -2;
                    break;
                case EndVariant.Right:
                    Utils.BuildTunnelAt(_game, 0, 0, ConnectorType.Right, true);
                    
                    while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && !tc.IsDeadlock && tc.Outs.Contains(ConnectorType.Left) && tc.Outs.Contains(ConnectorType.Up)) == 0)
                    {
                        _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
                    }
                    var tunnelCardRight = _game.CurrentPlayer.Hand
                            .Find(c => c is TunnelCard tc && !tc.IsDeadlock && tc.Outs.Contains(ConnectorType.Left) && tc.Outs.Contains(ConnectorType.Up))
                        as TunnelCard;
                    _game.ExecuteTurn(new BuildAction(tunnelCardRight, 1, 0, ConnectorType.Right));
                    
                    xBase = 2;
                    break;
                case EndVariant.Center:
                    xBase = 0;
                    break;
                default:
                    xBase = 0;
                    break;
            }

            var yBase = 0;
            while (yBase != 6)
            {
                Utils.BuildTunnelAt(_game, xBase, yBase, ConnectorType.Up, true);
                yBase++;
            }
            
            var turnResult = Utils.BuildTunnelAt(_game, xBase, yBase, ConnectorType.Up, true);
            
            Assert.IsInstanceOfType(turnResult, typeof(NewRoundResult), "TurnResult has failed");
            Assert.AreEqual(2, _game.Round, "Round count has failed");
            Assert.IsTrue(_game.Field.Start.Outs.All(o => o.Value == null), "Round count has failed");
            Assert.IsTrue(
                _game.Players.All(p => p.EndsStatuses.All(e => e.Value == TargetStatus.Unknow)), 
                "Players' end statuses has failed");
        }
    }
}