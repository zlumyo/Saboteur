using System.Collections.Generic;
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

            var turnResult = Utils.BuildTunnelAtBy(_game, 0, 0, ConnectorType.Right, false, currentPlayer);

            Assert.IsInstanceOfType(turnResult, typeof(UnacceptableActionResult));
        }
        
        [TestMethod]
        public void BanDeadlocksWhenWithoutDeadlocksTest()
        {
            _game = SaboteurGame.NewGame(true, false, MinPlayers);
            
            while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && tc.IsDeadlock && tc.Outs.Contains(ConnectorType.Down)) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var currentPlayer = _game.CurrentPlayer;
            var card = currentPlayer.Hand.Find(c => c is TunnelCard tc && tc.IsDeadlock && tc.Outs.Contains(ConnectorType.Down)) as TunnelCard;

            var turnResult = _game.ExecuteTurn(new BuildAction(card, 0, 0, ConnectorType.Up));

            Assert.IsInstanceOfType(turnResult, typeof(UnacceptableActionResult));
        }
        
        [TestMethod]
        public void PreventBuildNearNothingTest()
        {
            var turnResult = Utils.BuildTunnelAt(_game, 2, 0, ConnectorType.Right);

            Assert.IsInstanceOfType(turnResult, typeof(UnacceptableActionResult));
        }
        
        [TestMethod]
        public void PreventBuildWhereAlreadyBuiltTest()
        {
            Utils.BuildTunnelAt(_game, 0, 0, ConnectorType.Right);
            var turnResult = Utils.BuildTunnelAt(_game, 0, 0, ConnectorType.Right);

            Assert.IsInstanceOfType(turnResult, typeof(UnacceptableActionResult));
        }
        
        [TestMethod]
        public void PreventBuildWithLackOfConnectorTest()
        {
            while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && tc.Outs.Contains(ConnectorType.Left) && !tc.Outs.Contains(ConnectorType.Up) && !tc.Outs.Contains(ConnectorType.Down)) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var tunnelCard1 = _game.CurrentPlayer.Hand
                .Find(c => c is TunnelCard tc && tc.Outs.Contains(ConnectorType.Left) && !tc.Outs.Contains(ConnectorType.Up) && !tc.Outs.Contains(ConnectorType.Down))
                as TunnelCard;

            _game.ExecuteTurn(new BuildAction(tunnelCard1, 0, 0, ConnectorType.Right));
            
            while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && (tc.Outs.Contains(ConnectorType.Up) || tc.Outs.Contains(ConnectorType.Down))) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var tunnelCard2 = _game.CurrentPlayer.Hand
                    .Find(c => c is TunnelCard tc && (tc.Outs.Contains(ConnectorType.Up) || tc.Outs.Contains(ConnectorType.Down)))
                as TunnelCard;

            var turnResult = _game.ExecuteTurn(new BuildAction(tunnelCard2, 1, 0, ConnectorType.Up));

            Assert.IsInstanceOfType(turnResult, typeof(UnacceptableActionResult));
        }
        
        [TestMethod]
        public void AllowBuildWithFlipTest()
        {
            while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && tc.Outs.Contains(ConnectorType.Left) && tc.Outs.Contains(ConnectorType.Up)) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var tunnelCard1 = _game.CurrentPlayer.Hand
                    .Find(c => c is TunnelCard tc && tc.Outs.Contains(ConnectorType.Left) && tc.Outs.Contains(ConnectorType.Up))
                as TunnelCard;

            _game.ExecuteTurn(new BuildAction(tunnelCard1, 0, 0, ConnectorType.Right));
            
            while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && !tc.Outs.Contains(ConnectorType.Up) && tc.Outs.Contains(ConnectorType.Down)) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var tunnelCard2 = _game.CurrentPlayer.Hand
                    .Find(c => c is TunnelCard tc && !tc.Outs.Contains(ConnectorType.Up) && tc.Outs.Contains(ConnectorType.Down))
                as TunnelCard;

            var turnResult = _game.ExecuteTurn(new BuildAction(tunnelCard2, 1, 0, ConnectorType.Up));

            Assert.IsInstanceOfType(turnResult, typeof(NewTurnResult));
        }
        
        [TestMethod]
        public void PreventBuildWithWrongNeighborsTest()
        {
            while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && tc.Outs.Contains(ConnectorType.Left) && tc.Outs.Contains(ConnectorType.Up)) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var tunnelCard1 = _game.CurrentPlayer.Hand
                    .Find(c => c is TunnelCard tc && tc.Outs.Contains(ConnectorType.Left) && !tc.Outs.Contains(ConnectorType.Up))
                as TunnelCard;

            _game.ExecuteTurn(new BuildAction(tunnelCard1, 0, 0, ConnectorType.Right));
            
            while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && !tc.Outs.Contains(ConnectorType.Right) && tc.Outs.Contains(ConnectorType.Down)) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var tunnelCard2 = _game.CurrentPlayer.Hand
                    .Find(c => c is TunnelCard tc && !tc.Outs.Contains(ConnectorType.Right) && tc.Outs.Contains(ConnectorType.Down))
                as TunnelCard;

            _game.ExecuteTurn(new BuildAction(tunnelCard2, 0, 0, ConnectorType.Up));
            
            while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && tc.Outs.Contains(ConnectorType.Left) && tc.Outs.Contains(ConnectorType.Down)) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var tunnelCard3 = _game.CurrentPlayer.Hand
                    .Find(c => c is TunnelCard tc && tc.Outs.Contains(ConnectorType.Left) && tc.Outs.Contains(ConnectorType.Down))
                as TunnelCard;

            var turnResult = _game.ExecuteTurn(new BuildAction(tunnelCard3, 1, 0, ConnectorType.Up));

            Assert.IsInstanceOfType(turnResult, typeof(UnacceptableActionResult));
        }

        [TestMethod]
        public void FakeFinishOpening()
        {
            var direction = _game.Field.Ends.First(end => end.Value.Type == CellType.Fake).Key;
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
                    _game.ExecuteTurn(new BuildAction(tunnelCardRight, -1, 0, ConnectorType.Right));
                    
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
            while (yBase != 7)
            {
                Utils.BuildTunnelAt(_game, xBase, yBase, ConnectorType.Up, true);
                yBase++;
            }
            
            Assert.IsTrue(_game.Players.All(p => p.EndsStatuses[direction] == TargetStatus.Fake));
        }
        
        [TestMethod]
        public void AllNeighborsConnectedTest()
        {
            while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && tc.Outs.Contains(ConnectorType.Left) && tc.Outs.Contains(ConnectorType.Up)) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var tunnelCard1 = _game.CurrentPlayer.Hand
                    .Find(c => c is TunnelCard tc && tc.Outs.Contains(ConnectorType.Left) && tc.Outs.Contains(ConnectorType.Up))
                as TunnelCard;

            _game.ExecuteTurn(new BuildAction(tunnelCard1, 0, 0, ConnectorType.Right));
            
            while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && tc.Outs.Contains(ConnectorType.Right) && tc.Outs.Contains(ConnectorType.Down)) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var tunnelCard2 = _game.CurrentPlayer.Hand
                    .Find(c => c is TunnelCard tc && tc.Outs.Contains(ConnectorType.Right) && tc.Outs.Contains(ConnectorType.Down))
                as TunnelCard;

            _game.ExecuteTurn(new BuildAction(tunnelCard2, 0, 0, ConnectorType.Up));
            
            while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && tc.Outs.Contains(ConnectorType.Left) && tc.Outs.Contains(ConnectorType.Down)) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var tunnelCard3 = _game.CurrentPlayer.Hand
                    .Find(c => c is TunnelCard tc && tc.Outs.Contains(ConnectorType.Left) && tc.Outs.Contains(ConnectorType.Down))
                as TunnelCard;

            _game.ExecuteTurn(new BuildAction(tunnelCard3, 1, 0, ConnectorType.Up));

            var cellX0Y1 = _game.Field.Start.Outs[ConnectorType.Up];
            Assert.IsNotNull(cellX0Y1.Outs.GetValueOrDefault(ConnectorType.Right), "Nighbor didn't connected.");
        }
        
        [TestMethod]
        public void ConnectNeighborsOfFinishTest()
        {
            var direction = _game.Field.Ends.First(end => end.Value.Type == CellType.Fake).Key;
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
                    _game.ExecuteTurn(new BuildAction(tunnelCardRight, -1, 0, ConnectorType.Right));
                    
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
                        
            while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && !tc.IsDeadlock && !tc.Outs.Contains(ConnectorType.Up) && tc.Outs.Contains(ConnectorType.Down) && tc.Outs.Contains(ConnectorType.Right)) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }
            var card1 = _game.CurrentPlayer.Hand
                    .Find(c => c is TunnelCard tc && !tc.IsDeadlock && !tc.Outs.Contains(ConnectorType.Up) && tc.Outs.Contains(ConnectorType.Down) && tc.Outs.Contains(ConnectorType.Right))
                as TunnelCard;
            _game.ExecuteTurn(new BuildAction(card1, xBase, yBase, ConnectorType.Up));
            
            while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && !tc.IsDeadlock && tc.Outs.Contains(ConnectorType.Left) && tc.Outs.Contains(ConnectorType.Up)) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }
            var card2 = _game.CurrentPlayer.Hand
                    .Find(c => c is TunnelCard tc && !tc.IsDeadlock && tc.Outs.Contains(ConnectorType.Left) && tc.Outs.Contains(ConnectorType.Up))
                as TunnelCard;
            _game.ExecuteTurn(new BuildAction(card2, xBase, yBase + 1, ConnectorType.Right));
            
            while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && !tc.Outs.Contains(ConnectorType.Right) && tc.Outs.Contains(ConnectorType.Down) && tc.Outs.Contains(ConnectorType.Left)) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }
            var card3 = _game.CurrentPlayer.Hand
                    .Find(c => c is TunnelCard tc && !tc.Outs.Contains(ConnectorType.Right) && tc.Outs.Contains(ConnectorType.Down) && tc.Outs.Contains(ConnectorType.Left))
                as TunnelCard;
            _game.ExecuteTurn(new BuildAction(card3, xBase + 1, yBase + 1, ConnectorType.Up));
            
            Assert.AreEqual(1, _game.Field.Ends[direction].Outs.Count(p => p.Value != null), "Finish has extra links.");
        }
    }
}