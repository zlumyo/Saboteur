﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SaboteurFoundation;
using SaboteurFoundation.Cards;
using SaboteurFoundation.Turn;
using System.Linq;

namespace SaboteurTest
{
    [TestClass]
    public class SaboteurExecuteTurnTest
    {
        private static readonly string[] MinPlayers = { "player1", "player2", "player3" };

        private SaboteurGame _game;

        [TestInitialize]
        public void TestInit()
        {
            _game = SaboteurGame.NewGame(false, false, MinPlayers);
        }

        [TestMethod]
        public void SkipTurnTest()
        {
            var currentPlayer = _game.CurrentPlayer;
            var expectedCard = _game.Deck.Peek();
            var expectedDeckSize = _game.Deck.Count - 1;
            var expectedCardCount = 1 + currentPlayer.Hand.Count(c => c.Equals(expectedCard));
            if (currentPlayer.Hand.First().Equals(expectedCard))
            {
                expectedCardCount--;
            }
            
            _game.ExecuteTurn(new SkipAction(currentPlayer.Hand.First()));

            Assert.AreEqual(expectedDeckSize, _game.Deck.Count, "New deck's size has failed.");
            Assert.AreEqual(6, currentPlayer.Hand.Count, "New hand's size has failed.");
            Assert.AreEqual(expectedCardCount, currentPlayer.Hand.Count(c => c.Equals(expectedCard)), "New card's count has failed.");
        }

        [TestMethod]
        public void InvestigateFinishTest()
        {
            while (_game.CurrentPlayer.Hand.Count(c => c is InvestigateCard) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var currentPlayer = _game.CurrentPlayer;
            var card = currentPlayer.Hand.Find(c => c is InvestigateCard) as InvestigateCard;

            _game.ExecuteTurn(new PlayInvestigateAction(card, EndVariant.CENTER));

            Assert.AreNotEqual(TargetStatus.UNKNOW, currentPlayer.EndsStatuses[EndVariant.CENTER], "New end's status has failed.");
        }

        [TestMethod]
        public void DebufPlayerTest()
        {
            while (_game.CurrentPlayer.Hand.Count(c => c is DebufCard) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var currentPlayer = _game.CurrentPlayer;
            var card = currentPlayer.Hand.Find(c => c is DebufCard) as DebufCard;

            _game.ExecuteTurn(new PlayDebufAction(card, currentPlayer));

            Assert.IsTrue(card != null && currentPlayer.Debufs.Contains(card.Debuf) && currentPlayer.Debufs.Count == 1, "Debufs' state has failed.");
        }


        [TestMethod]
        public void HealPlayerTest()
        {
            while (_game.CurrentPlayer.Hand.Count(c => c is DebufCard) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var player1 = _game.CurrentPlayer;
            if (!(player1.Hand.Find(c => c is DebufCard) is DebufCard debufCard)) throw new ArgumentNullException(nameof(debufCard));
            _game.ExecuteTurn(new PlayDebufAction(debufCard, player1));

            while (_game.CurrentPlayer.Hand.Count(c => c is HealCard hc && hc.Heal == debufCard.Debuf) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var player2 = _game.CurrentPlayer;
            var healCard = player2.Hand.Find(c => c is HealCard hc && hc.Heal == debufCard.Debuf) as HealCard;

            _game.ExecuteTurn(new PlayBufAction(healCard, player1));

            Assert.IsTrue(player1.Debufs.Count == 0, "Debufs' state has failed.");
        }

        [TestMethod]
        public void AlternativeHealPlayerTest()
        {
            while (_game.CurrentPlayer.Hand.Count(c => c is DebufCard) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var player1 = _game.CurrentPlayer;
            if (!(player1.Hand.Find(c => c is DebufCard) is DebufCard debufCard)) throw new ArgumentNullException(nameof(debufCard));
            _game.ExecuteTurn(new PlayDebufAction(debufCard, player1));

            while (_game.CurrentPlayer.Hand.Count(c => c is HealAlternativeCard hc && (hc.HealAlternative1 == debufCard.Debuf || hc.HealAlternative2 == debufCard.Debuf)) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var player2 = _game.CurrentPlayer;
            var healCard = player2.Hand.Find(c => c is HealAlternativeCard hc && (hc.HealAlternative1 == debufCard.Debuf || hc.HealAlternative2 == debufCard.Debuf)) as HealAlternativeCard;

            _game.ExecuteTurn(new PlayBufAlternativeAction(healCard, player1));

            Assert.IsTrue(player1.Debufs.Count == 0, "Debufs' state has failed.");
        }

        [TestMethod]
        public void BuildTunnelTest()
        {
            BuildTunnelAt(0, 0, ConnectorType.RIGHT);

            var rightConnectorOfStart = _game.Field.Start.Outs.First(c => c.Type == ConnectorType.RIGHT);
            Assert.IsNotNull(rightConnectorOfStart.Next, "Field's state has failed.");
            var leftConnectorOfNext = rightConnectorOfStart.Next.Outs.First(c => c.Type == ConnectorType.LEFT);
            Assert.IsNotNull(leftConnectorOfNext.Next, "Double link is missed.");
        }

        [TestMethod]
        public void CollapseTunnelTest()
        {
            BuildTunnelAt(0, 0, ConnectorType.RIGHT);

            while (_game.CurrentPlayer.Hand.Count(c => c is CollapseCard) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var collapseCard = _game.CurrentPlayer.Hand.Find(c => c is CollapseCard) as CollapseCard;

            _game.ExecuteTurn(new CollapseAction(collapseCard, 1, 0));

            var rightConnectorOfStart = _game.Field.Start.Outs.First(c => c.Type == ConnectorType.RIGHT);
            Assert.IsTrue(rightConnectorOfStart.Next.HasCollapsed, "Cell's state has failed.");
        }

        [TestMethod]
        public void MissedCardTest()
        {
            while (_game.CurrentPlayer.Hand.Count(c => c is InvestigateCard) != 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }
            
            var turnResult = _game.ExecuteTurn(new PlayInvestigateAction(new InvestigateCard(), EndVariant.CENTER));
            
            Assert.IsInstanceOfType(turnResult, typeof(UnacceptableActionResult));
        }
        
        [TestMethod]
        public void CollapseStartTest()
        {
            while (_game.CurrentPlayer.Hand.Count(c => c is CollapseCard) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }
            
            var collapseCard = _game.CurrentPlayer.Hand.Find(c => c is CollapseCard) as CollapseCard;
            var turnResult = _game.ExecuteTurn(new CollapseAction(collapseCard, 0, 0));
            
            Assert.IsInstanceOfType(turnResult, typeof(UnacceptableActionResult));
        }
        
        [TestMethod]
        public void CollapseNonexistingTest()
        {
            while (_game.CurrentPlayer.Hand.Count(c => c is CollapseCard) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }
            
            var collapseCard = _game.CurrentPlayer.Hand.Find(c => c is CollapseCard) as CollapseCard;
            var turnResult = _game.ExecuteTurn(new CollapseAction(collapseCard, 1, 0));
            
            Assert.IsInstanceOfType(turnResult, typeof(UnacceptableActionResult));
        }
        
        [TestMethod]
        public void CollapseAlreadyCollapsedTest()
        {
            BuildTunnelAt(0, 0, ConnectorType.RIGHT);
            
            while (_game.CurrentPlayer.Hand.Count(c => c is CollapseCard) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }
            
            var collapseCard1 = _game.CurrentPlayer.Hand.Find(c => c is CollapseCard) as CollapseCard;
            _game.ExecuteTurn(new CollapseAction(collapseCard1, 1, 0));
            
            while (_game.CurrentPlayer.Hand.Count(c => c is CollapseCard) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }
            
            var collapseCard2 = _game.CurrentPlayer.Hand.Find(c => c is CollapseCard) as CollapseCard;
            var turnResult = _game.ExecuteTurn(new CollapseAction(collapseCard2, 1, 0));
            
            Assert.IsInstanceOfType(turnResult, typeof(UnacceptableActionResult));
        }
        
        [TestMethod]
        public void CollapseExitTest()
        {
            var direction = _game.Field.Ends.First(end => end.Value.Type == CellType.FAKE).Key;
            int xBase;
            switch (direction)
            {
                case EndVariant.LEFT:
                    BuildTunnelAt(0, 0, ConnectorType.LEFT, true);
                    BuildTunnelAt(-1, 0, ConnectorType.LEFT);
                    xBase = -2;
                    break;
                case EndVariant.RIGHT:
                    BuildTunnelAt(0, 0, ConnectorType.LEFT, true);
                    BuildTunnelAt(-1, 0, ConnectorType.LEFT);
                    xBase = 2;
                    break;
                case EndVariant.CENTER:
                    xBase = 0;
                    break;
                default:
                    xBase = 0;
                    break;
            }

            var yBase = 0;
            while (yBase != 8)
            {
                BuildTunnelAt(xBase, yBase, ConnectorType.UP, true);
                yBase++;
            }        
            
            while (_game.CurrentPlayer.Hand.Count(c => c is CollapseCard) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }
            
            var collapseCard = _game.CurrentPlayer.Hand.Find(c => c is CollapseCard) as CollapseCard;
            var turnResult = _game.ExecuteTurn(new CollapseAction(collapseCard, xBase, yBase));
            
            Assert.IsInstanceOfType(turnResult, typeof(UnacceptableActionResult));
        }
        
        [TestMethod]
        public void DebufAlreadyDebufedTest()
        {
            while (_game.CurrentPlayer.Hand.Count(c => c is DebufCard) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var debufedPlayer = _game.CurrentPlayer;
            if (!(debufedPlayer.Hand.Find(c => c is DebufCard) is DebufCard card1)) throw new ArgumentNullException(nameof(card1));

            _game.ExecuteTurn(new PlayDebufAction(card1, debufedPlayer));
            
            while (_game.CurrentPlayer.Hand.Count(c => c is DebufCard dc && dc.Debuf == card1.Debuf) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var anotherPlayer = _game.CurrentPlayer;
            var card2 = anotherPlayer.Hand.Find(c => c is DebufCard dc && dc.Debuf == card1.Debuf) as DebufCard;
            
            var turnResult =_game.ExecuteTurn(new PlayDebufAction(card2, debufedPlayer));

            Assert.IsInstanceOfType(turnResult, typeof(UnacceptableActionResult));
        }
        
        private void BuildTunnelAt(int x, int y, ConnectorType side, bool withOppositeSide = false)
        {
            var flippedSide = Connector.FlipConnectorType(side);
            while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && tc.Outs.Contains(flippedSide) && (!withOppositeSide || tc.Outs.Contains(side))) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var tunnelCard = _game.CurrentPlayer.Hand.Find(c => c is TunnelCard tc && tc.Outs.Contains(flippedSide)) as TunnelCard;

            _game.ExecuteTurn(new BuildAction(tunnelCard, x, y, side));
        }
    }
}
