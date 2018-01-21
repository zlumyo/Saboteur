using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SaboteurFoundation;
using SaboteurFoundation.Cards;
using SaboteurFoundation.Turn;

namespace SaboteurTest
{
    /// <summary>
    /// Тесты ситуаций возникающих при строительстве туннелей.
    /// </summary>
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
        
        /// <summary>
        /// Проверка невозможности строительства при наличии дебафа у игрока.
        /// </summary>
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

            var turnResult = Utils.BuildTunnelAtBy(_game, 1, 0, builder: currentPlayer);

            Assert.IsInstanceOfType(turnResult, typeof(UnacceptableActionResult));
        }
        
        /// <summary>
        /// Проверка невозможности построить тупик при включении соответствующего правила.
        /// </summary>
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

            var turnResult = _game.ExecuteTurn(new BuildAction(card, 0, 1));

            Assert.IsInstanceOfType(turnResult, typeof(UnacceptableActionResult));
        }
        
        /// <summary>
        /// Проверка невозможности построить туннель в клетке, рядом с которой нет построек.
        /// </summary>
        [TestMethod]
        public void PreventBuildNearNothingTest()
        {
            var turnResult = Utils.BuildTunnelAt(_game, 3, 0);

            Assert.IsInstanceOfType(turnResult, typeof(UnacceptableActionResult));
        }
        
        /// <summary>
        /// Проверка невозможности построить туннель в клетке, которая уже занята.
        /// </summary>
        [TestMethod]
        public void PreventBuildWhereAlreadyBuiltTest()
        {
            Utils.BuildTunnelAt(_game, 1, 0);
            var turnResult = Utils.BuildTunnelAt(_game, 1, 0);

            Assert.IsInstanceOfType(turnResult, typeof(UnacceptableActionResult));
        }
        
        /// <summary>
        /// Проверка невозможности построить туннель в клетке, рядом с которой нет подходящих соединений.
        /// </summary>
        [TestMethod]
        public void PreventBuildWithLackOfConnectorTest()
        {
            while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && tc.Outs.Contains(ConnectorType.Left) && !tc.Outs.Contains(ConnectorType.Up)) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var tunnelCard1 = _game.CurrentPlayer.Hand
                .Find(c => c is TunnelCard tc && tc.Outs.Contains(ConnectorType.Left) && !tc.Outs.Contains(ConnectorType.Up))
                as TunnelCard;

            _game.ExecuteTurn(new BuildAction(tunnelCard1, 1, 0));
            
            while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && (tc.Outs.Contains(ConnectorType.Up) || tc.Outs.Contains(ConnectorType.Down))) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var tunnelCard2 = _game.CurrentPlayer.Hand
                    .Find(c => c is TunnelCard tc && (tc.Outs.Contains(ConnectorType.Up) || tc.Outs.Contains(ConnectorType.Down)))
                as TunnelCard;

            var turnResult = _game.ExecuteTurn(new BuildAction(tunnelCard2, 1, 1));

            Assert.IsInstanceOfType(turnResult, typeof(UnacceptableActionResult));
        }
        
        /// <summary>
        /// Проверка возможности построить туннель, если он подходит при переворачивании.
        /// </summary>
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

            _game.ExecuteTurn(new BuildAction(tunnelCard1, 1, 0));
            
            while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && !tc.Outs.Contains(ConnectorType.Up) && tc.Outs.Contains(ConnectorType.Down)) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var tunnelCard2 = _game.CurrentPlayer.Hand
                    .Find(c => c is TunnelCard tc && !tc.Outs.Contains(ConnectorType.Up) && tc.Outs.Contains(ConnectorType.Down))
                as TunnelCard;

            var turnResult = _game.ExecuteTurn(new BuildAction(tunnelCard2, 1, 1));

            Assert.IsInstanceOfType(turnResult, typeof(NewTurnResult));
        }
        
        /// <summary>
        /// Проверка невозможности построить туннель в случае неподходящих "соседей".
        /// </summary>
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

            _game.ExecuteTurn(new BuildAction(tunnelCard1, 1, 0));
            
            while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && !tc.Outs.Contains(ConnectorType.Right) && tc.Outs.Contains(ConnectorType.Down)) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var tunnelCard2 = _game.CurrentPlayer.Hand
                    .Find(c => c is TunnelCard tc && !tc.Outs.Contains(ConnectorType.Right) && tc.Outs.Contains(ConnectorType.Down))
                as TunnelCard;

            _game.ExecuteTurn(new BuildAction(tunnelCard2, 0, 1));
            
            while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && tc.Outs.Contains(ConnectorType.Left) && tc.Outs.Contains(ConnectorType.Down)) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var tunnelCard3 = _game.CurrentPlayer.Hand
                    .Find(c => c is TunnelCard tc && tc.Outs.Contains(ConnectorType.Left) && tc.Outs.Contains(ConnectorType.Down))
                as TunnelCard;

            var turnResult = _game.ExecuteTurn(new BuildAction(tunnelCard3, 1, 1));

            Assert.IsInstanceOfType(turnResult, typeof(UnacceptableActionResult));
        }

        /// <summary>
        /// Проверка открытия ложного золота для всех игроков при его достижении.
        /// </summary>
        [TestMethod]
        public void FakeFinishOpening()
        {
            var direction = _game.Field.Ends.First(end => end.Value.Type == CellType.Fake).Key;
            int xBase;
            switch (direction)
            {
                case EndVariant.Left:
                    Utils.BuildTunnelAt(_game, -1, 0, new HashSet<ConnectorType> { ConnectorType.Left, ConnectorType.Right });
                    
                    while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && !tc.IsDeadlock && tc.Outs.Contains(ConnectorType.Right) && tc.Outs.Contains(ConnectorType.Up)) == 0)
                    {
                        _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
                    }
                    var tunnelCardLeft = _game.CurrentPlayer.Hand
                            .Find(c => c is TunnelCard tc && !tc.IsDeadlock && tc.Outs.Contains(ConnectorType.Right) && tc.Outs.Contains(ConnectorType.Up))
                        as TunnelCard;
                    _game.ExecuteTurn(new BuildAction(tunnelCardLeft, -2, 0));
                    
                    xBase = -2;
                    break;
                case EndVariant.Right:
                    Utils.BuildTunnelAt(_game, 1, 0, new HashSet<ConnectorType> { ConnectorType.Left, ConnectorType.Right });
                    
                    while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && !tc.IsDeadlock && tc.Outs.Contains(ConnectorType.Left) && tc.Outs.Contains(ConnectorType.Up)) == 0)
                    {
                        _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
                    }
                    var tunnelCardRight = _game.CurrentPlayer.Hand
                            .Find(c => c is TunnelCard tc && !tc.IsDeadlock && tc.Outs.Contains(ConnectorType.Left) && tc.Outs.Contains(ConnectorType.Up))
                        as TunnelCard;
                    _game.ExecuteTurn(new BuildAction(tunnelCardRight, 2, 0));
                    
                    xBase = 2;
                    break;
                case EndVariant.Center:
                    xBase = 0;
                    break;
                default:
                    xBase = 0;
                    break;
            }

            var yBase = 1;
            while (yBase <= 7)
            {
                Utils.BuildTunnelAt(_game, xBase, yBase, new HashSet<ConnectorType> { ConnectorType.Up, ConnectorType.Down });
                yBase++;
            }
            
            Assert.IsTrue(_game.Players.All(p => p.EndsStatuses[direction] == TargetStatus.Fake));
        }
        
        /// <summary>
        /// Проверка наличия всех соединений со всеми соседями, при успешной постройке туннеля.
        /// </summary>
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

            _game.ExecuteTurn(new BuildAction(tunnelCard1, 1, 0));
            
            while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && tc.Outs.Contains(ConnectorType.Right) && tc.Outs.Contains(ConnectorType.Down)) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var tunnelCard2 = _game.CurrentPlayer.Hand
                    .Find(c => c is TunnelCard tc && tc.Outs.Contains(ConnectorType.Right) && tc.Outs.Contains(ConnectorType.Down))
                as TunnelCard;

            _game.ExecuteTurn(new BuildAction(tunnelCard2, 0, 1));
            
            while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && tc.Outs.Contains(ConnectorType.Left) && tc.Outs.Contains(ConnectorType.Down)) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var tunnelCard3 = _game.CurrentPlayer.Hand
                    .Find(c => c is TunnelCard tc && tc.Outs.Contains(ConnectorType.Left) && tc.Outs.Contains(ConnectorType.Down))
                as TunnelCard;

            _game.ExecuteTurn(new BuildAction(tunnelCard3, 1, 1));

            var cellX0Y1 = _game.Field.Start.Outs[ConnectorType.Up];
            Assert.IsNotNull(cellX0Y1.Outs.GetValueOrDefault(ConnectorType.Right), "Nighbor didn't connected.");
        }
        
        /// <summary>
        /// Проверка наличия всех соединений с соседями, при открытии фейкового золота.
        /// </summary>
        [TestMethod]
        public void ConnectNeighborsOfFinishTest()
        {
            var direction = _game.Field.Ends.First(end => end.Value.Type == CellType.Fake).Key;
            int xBase;
            switch (direction)
            {
                case EndVariant.Left:
                    Utils.BuildTunnelAt(_game, -1, 0, new HashSet<ConnectorType> { ConnectorType.Right, ConnectorType.Left });
                    
                    while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && !tc.IsDeadlock && tc.Outs.Contains(ConnectorType.Right) && tc.Outs.Contains(ConnectorType.Up)) == 0)
                    {
                        _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
                    }
                    var tunnelCardLeft = _game.CurrentPlayer.Hand
                            .Find(c => c is TunnelCard tc && !tc.IsDeadlock && tc.Outs.Contains(ConnectorType.Right) && tc.Outs.Contains(ConnectorType.Up))
                        as TunnelCard;
                    _game.ExecuteTurn(new BuildAction(tunnelCardLeft, -2, 0));
                    
                    xBase = -2;
                    break;
                case EndVariant.Right:
                    Utils.BuildTunnelAt(_game, 1, 0, new HashSet<ConnectorType> { ConnectorType.Right, ConnectorType.Left });
                    
                    while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && !tc.IsDeadlock && tc.Outs.Contains(ConnectorType.Left) && tc.Outs.Contains(ConnectorType.Up)) == 0)
                    {
                        _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
                    }
                    var tunnelCardRight = _game.CurrentPlayer.Hand
                            .Find(c => c is TunnelCard tc && !tc.IsDeadlock && tc.Outs.Contains(ConnectorType.Left) && tc.Outs.Contains(ConnectorType.Up))
                        as TunnelCard;
                    _game.ExecuteTurn(new BuildAction(tunnelCardRight, 2, 0));
                    
                    xBase = 2;
                    break;
                case EndVariant.Center:
                    xBase = 0;
                    break;
                default:
                    xBase = 0;
                    break;
            }

            var yBase = 1;
            while (yBase != 7)
            {
                Utils.BuildTunnelAt(_game, xBase, yBase, new HashSet<ConnectorType> { ConnectorType.Up, ConnectorType.Down });
                yBase++;
            }
                        
            while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && !tc.IsDeadlock && !tc.Outs.Contains(ConnectorType.Up) && tc.Outs.Contains(ConnectorType.Down) && tc.Outs.Contains(ConnectorType.Right)) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }
            var card1 = _game.CurrentPlayer.Hand
                    .Find(c => c is TunnelCard tc && !tc.IsDeadlock && !tc.Outs.Contains(ConnectorType.Up) && tc.Outs.Contains(ConnectorType.Down) && tc.Outs.Contains(ConnectorType.Right))
                as TunnelCard;
            _game.ExecuteTurn(new BuildAction(card1, xBase, yBase));
            
            while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && !tc.IsDeadlock && tc.Outs.Contains(ConnectorType.Left) && tc.Outs.Contains(ConnectorType.Up)) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }
            var card2 = _game.CurrentPlayer.Hand
                    .Find(c => c is TunnelCard tc && !tc.IsDeadlock && tc.Outs.Contains(ConnectorType.Left) && tc.Outs.Contains(ConnectorType.Up))
                as TunnelCard;
            _game.ExecuteTurn(new BuildAction(card2, xBase+1, yBase));
            
            while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && !tc.Outs.Contains(ConnectorType.Right) && tc.Outs.Contains(ConnectorType.Down) && tc.Outs.Contains(ConnectorType.Left)) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }
            var card3 = _game.CurrentPlayer.Hand
                    .Find(c => c is TunnelCard tc && !tc.Outs.Contains(ConnectorType.Right) && tc.Outs.Contains(ConnectorType.Down) && tc.Outs.Contains(ConnectorType.Left))
                as TunnelCard;
            _game.ExecuteTurn(new BuildAction(card3, xBase + 1, yBase + 1));
            
            Assert.AreEqual(1, _game.Field.Ends[direction].Outs.Count(p => p.Value != null), "Finish has extra links.");
        }
    }
}