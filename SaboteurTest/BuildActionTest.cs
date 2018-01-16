using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SaboteurFoundation;
using SaboteurFoundation.Cards;
using SaboteurFoundation.Turn;

namespace SaboteurTest
{
    [TestClass]
    public class BuildActionTest
    {
        private static readonly string[] MinPlayers = { "player1", "player2", "player3" };
        
        private SaboteurGame _game;
        
        [TestInitialize]
        public void TestInit()
        {
            _game = SaboteurGame.NewGame(false, false, MinPlayers);
        }
        
        [TestMethod]
        public void BanBuildingWhenDebufedTest()
        {
            while (_game.CurrentPlayer.Hand.Count(c => c is DebufCard) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var currentPlayer = _game.CurrentPlayer;
            var card = currentPlayer.Hand.Find(c => c is DebufCard) as DebufCard;

            _game.ExecuteTurn(new PlayDebufAction(card, currentPlayer));

            var turnResult = Utils.BuildTunnelAtBy(_game, 1, 0, ConnectorType.RIGHT, false, currentPlayer);

            Assert.IsInstanceOfType(turnResult, typeof(UnacceptableActionResult));
        }
        
        [TestMethod]
        public void BanDeadlocksWhenWithoutDeadlocksTest()
        {
            _game = SaboteurGame.NewGame(true, false, MinPlayers);
            
            while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && tc.IsDeadlock && tc.Outs.Contains(ConnectorType.DOWN)) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var currentPlayer = _game.CurrentPlayer;
            var card = currentPlayer.Hand.Find(c => c is TunnelCard tc && tc.IsDeadlock && tc.Outs.Contains(ConnectorType.DOWN)) as TunnelCard;

            var turnResult = _game.ExecuteTurn(new BuildAction(card, 0, 1, ConnectorType.UP));

            Assert.IsInstanceOfType(turnResult, typeof(UnacceptableActionResult));
        }
    }
}