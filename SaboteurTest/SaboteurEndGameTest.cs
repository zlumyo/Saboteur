using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SaboteurFoundation;
using SaboteurFoundation.Cards;
using SaboteurFoundation.Turn;

namespace SaboteurTest
{
    /// <summary>
    /// Тесты сценариев связанных с окончанием раунда и/или игры.
    /// </summary>
    [TestClass]
    public class SaboteurEndGameTest
    {
        private static readonly string[] MinPlayers = { "player1", "player2", "player3" };

        private SaboteurGame _game;

        [TestInitialize]
        public void TestInit()
        {
            _game = SaboteurGame.NewGame(false, true, MinPlayers);
        }
        
        /// <summary>
        /// Проверка состояния игры по завершении раунда.
        /// </summary>
        [TestMethod]
        public void NextRoundTest()
        {
            var direction = _game.Field.Ends.First(end => end.Value.Type == CellType.Gold).Key;
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
            while (yBase != 7)
            {
                Utils.BuildTunnelAt(_game, xBase, yBase, new HashSet<ConnectorType> { ConnectorType.Up, ConnectorType.Down });
                yBase++;
            }
            
            var turnResult = Utils.BuildTunnelAt(_game, xBase, yBase, new HashSet<ConnectorType> { ConnectorType.Up, ConnectorType.Down });
            
            Assert.IsInstanceOfType(turnResult, typeof(NewRoundResult), "TurnResult has failed");
            Assert.AreEqual(2, _game.Round, "Round count has failed");
            Assert.IsTrue(_game.Field.Start.Outs.All(o => o.Value == null), "Start cell's state has failed");
            Assert.IsTrue(
                _game.Players.All(p => p.EndsStatuses.All(e => e.Value == TargetStatus.Unknow)), 
                "Players' end statuses has failed");
        }
        
        /// <summary>
        /// Проверка сотояния игры по завершении всех раундов.
        /// </summary>
        [TestMethod]
        public void EndOfGameTest()
        {
            TurnResult turnResult = default;
            
            for (var i = 0; i < SaboteurGame.RoundsInGame; i++)
            {
                var direction = _game.Field.Ends.First(end => end.Value.Type == CellType.Gold).Key;
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
                
                turnResult = Utils.BuildTunnelAt(_game, xBase, yBase, new HashSet<ConnectorType> { ConnectorType.Up, ConnectorType.Down });
            }
            
                     
            Assert.IsInstanceOfType(turnResult, typeof(EndGameResult), "TurnResult has failed");
            Assert.AreEqual(3, _game.Round, "Round count has failed");
            Assert.IsInstanceOfType(_game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First())),
                typeof(EndGameResult),
                "Execute turn after end of game has failed");
            Assert.IsTrue(_game.IsGameEnded, "IsGameEnded state has failed");
        }
        
        /// <summary>
        /// Проверка распределения золота при победе "хороших парней".
        /// </summary>
        [TestMethod]
        public void GoodBoysGetGoldTest()
        {
            var direction = _game.Field.Ends.First(end => end.Value.Type == CellType.Gold).Key;
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
            
            Utils.BuildTunnelAt(_game, xBase, yBase, new HashSet<ConnectorType> { ConnectorType.Up, ConnectorType.Down });
            
            Assert.AreEqual(28-_game.Players.Count, _game.GoldHeap.Count);
        }
        
        /// <summary>
        /// Проверка распределения золота при победе "плохих парней".
        /// </summary>
        [TestMethod]
        public void BadBoysGetGoldTest()
        {
            while (_game.Players.Count(p => p.Role == GameRole.Bad) == 0)
            {
                _game = SaboteurGame.NewGame(false, false, MinPlayers);
            }
            
            TurnResult turnResult;
            do
            {
                turnResult = _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            } while (!(turnResult is NewRoundResult));
            
            
            Assert.AreNotEqual(0, _game.Players.Count(p => p.Gold != 0));
        }
        
        /// <summary>
        /// Проверка отсутствия золота у игроков под дебафом(-ами) при включении соответствующего правила.
        /// </summary>
        [TestMethod]
        public void DebufedDoesntGetGoldTest()
        {      
            while (_game.Players.Count(p => p.Role == GameRole.Bad) == 0)
            {
                _game = SaboteurGame.NewGame(false, true, MinPlayers);
            }

            foreach (var player in _game.Players)
            {
                if (player.Role == GameRole.Bad)
                    player.Debufs.Add(Effect.Lamp);
            }
            
            TurnResult turnResult;
            do
            {
                turnResult = _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            } while (!(turnResult is NewRoundResult));
            
            
            Assert.AreEqual(0, _game.Players.Count(p => p.Gold != 0));
        }
    }
}