﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SaboteurFoundation;
using SaboteurFoundation.Cards;
using SaboteurFoundation.Turn;
using System.Linq;

namespace SaboteurTest
{
    /// <summary>
    /// Тесты ходов всеми картами.
    /// </summary>
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

        /// <summary>
        /// Пропуск хода.
        /// </summary>
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

        /// <summary>
        /// Карта "подсмотреть золотую жилу".
        /// </summary>
        [TestMethod]
        public void InvestigateFinishTest()
        {
            while (_game.CurrentPlayer.Hand.Count(c => c is InvestigateCard) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var currentPlayer = _game.CurrentPlayer;
            var card = currentPlayer.Hand.Find(c => c is InvestigateCard) as InvestigateCard;

            _game.ExecuteTurn(new InvestigateAction(card, EndVariant.Center));

            Assert.AreNotEqual(TargetStatus.Unknow, currentPlayer.EndsStatuses[EndVariant.Center], "New end's status has failed.");
        }

        /// <summary>
        /// Карта "поломать что-то".
        /// </summary>
        [TestMethod]
        public void DebufPlayerTest()
        {
            while (_game.CurrentPlayer.Hand.Count(c => c is DebufCard) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var currentPlayer = _game.CurrentPlayer;
            var card = currentPlayer.Hand.Find(c => c is DebufCard) as DebufCard;

            _game.ExecuteTurn(new DebufAction(card, currentPlayer));

            Assert.IsTrue(card != null && currentPlayer.Debufs.Contains(card.Debuf) && currentPlayer.Debufs.Count == 1, "Debufs' state has failed.");
        }

        /// <summary>
        /// Карта "починить что-то".
        /// </summary>
        [TestMethod]
        public void HealPlayerTest()
        {
            while (_game.CurrentPlayer.Hand.Count(c => c is DebufCard) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var player1 = _game.CurrentPlayer;
            if (!(player1.Hand.Find(c => c is DebufCard) is DebufCard debufCard)) throw new ArgumentNullException(nameof(debufCard));
            _game.ExecuteTurn(new DebufAction(debufCard, player1));

            while (_game.CurrentPlayer.Hand.Count(c => c is HealCard hc && hc.Heal == debufCard.Debuf) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var player2 = _game.CurrentPlayer;
            var healCard = player2.Hand.Find(c => c is HealCard hc && hc.Heal == debufCard.Debuf) as HealCard;

            _game.ExecuteTurn(new HealAction(healCard, player1));

            Assert.IsTrue(player1.Debufs.Count == 0, "Debufs' state has failed.");
        }

        /// <summary>
        /// Карта "починить что-то из двух".
        /// </summary>
        [TestMethod]
        public void AlternativeHealPlayerTest()
        {
            while (_game.CurrentPlayer.Hand.Count(c => c is DebufCard) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var player1 = _game.CurrentPlayer;
            if (!(player1.Hand.Find(c => c is DebufCard) is DebufCard debufCard)) throw new ArgumentNullException(nameof(debufCard));
            _game.ExecuteTurn(new DebufAction(debufCard, player1));

            while (_game.CurrentPlayer.Hand.Count(c => c is HealAlternativeCard hc && (hc.HealAlternative1 == debufCard.Debuf || hc.HealAlternative2 == debufCard.Debuf)) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var player2 = _game.CurrentPlayer;
            var healCard = player2.Hand.Find(c => c is HealAlternativeCard hc && (hc.HealAlternative1 == debufCard.Debuf || hc.HealAlternative2 == debufCard.Debuf)) as HealAlternativeCard;

            _game.ExecuteTurn(new HealAlternativeAction(healCard, player1));

            Assert.IsTrue(player1.Debufs.Count == 0, "Debufs' state has failed.");
        }

        /// <summary>
        /// Карта "построить туннель".
        /// </summary>
        [TestMethod]
        public void BuildTunnelTest()
        {
            Utils.BuildTunnelAt(_game, 1, 0);

            Assert.IsTrue(_game.Field.Start.Outs.TryGetValue(ConnectorType.Right, out var rightConnectorOfStart) && rightConnectorOfStart != null, "Field's state has failed.");
            Assert.IsTrue(rightConnectorOfStart.Outs.TryGetValue(ConnectorType.Left, out var leftConnectorOfNext) && leftConnectorOfNext != null, "Double link is missed.");
        }

        /// <summary>
        /// Карта "обрушить туннель".
        /// </summary>
        [TestMethod]
        public void CollapseTunnelTest()
        {
            Utils.BuildTunnelAt(_game, 1, 0);

            while (_game.CurrentPlayer.Hand.Count(c => c is CollapseCard) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var collapseCard = _game.CurrentPlayer.Hand.Find(c => c is CollapseCard) as CollapseCard;

            _game.ExecuteTurn(new CollapseAction(collapseCard, 1, 0));

            Assert.IsTrue(_game.Field.Start.Outs[ConnectorType.Right].HasCollapsed, "Cell's state has failed.");
        }

        /// <summary>
        /// Попытка использовать карту, которой нет в руке.
        /// </summary>
        [TestMethod]
        public void MissedCardTest()
        {
            while (_game.CurrentPlayer.Hand.Count(c => c is InvestigateCard) != 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }
            
            var turnResult = _game.ExecuteTurn(new InvestigateAction(new InvestigateCard(), EndVariant.Center));
            
            Assert.IsInstanceOfType(turnResult, typeof(UnacceptableActionResult));
        }
        
        /// <summary>
        /// Попытка обрушить "вход" на игровом поле.
        /// </summary>
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
        
        /// <summary>
        /// Попытка обрушить несуществующий туннель.
        /// </summary>
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
        
        /// <summary>
        /// Попытка обрушить уже обрушенный туннель.
        /// </summary>
        [TestMethod]
        public void CollapseAlreadyCollapsedTest()
        {
            Utils.BuildTunnelAt(_game, 1, 0);
            
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
        
        /// <summary>
        /// Попытка поломать у игрока один и тот же инструмент дважды.
        /// </summary>
        [TestMethod]
        public void DebufAlreadyDebufedTest()
        {
            while (_game.CurrentPlayer.Hand.Count(c => c is DebufCard) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var debufedPlayer = _game.CurrentPlayer;
            if (!(debufedPlayer.Hand.Find(c => c is DebufCard) is DebufCard card1)) throw new ArgumentNullException(nameof(card1));

            _game.ExecuteTurn(new DebufAction(card1, debufedPlayer));
            
            while (_game.CurrentPlayer.Hand.Count(c => c is DebufCard dc && dc.Debuf == card1.Debuf) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var anotherPlayer = _game.CurrentPlayer;
            var card2 = anotherPlayer.Hand.Find(c => c is DebufCard dc && dc.Debuf == card1.Debuf) as DebufCard;
            
            var turnResult = _game.ExecuteTurn(new DebufAction(card2, debufedPlayer));

            Assert.IsInstanceOfType(turnResult, typeof(UnacceptableActionResult));
        }
        
        /// <summary>
        /// Попытка починить поломку не той картой.
        /// </summary>
        [TestMethod]
        public void HealWrongDebufTest()
        {
            while (_game.CurrentPlayer.Hand.Count(c => c is DebufCard) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var debufedPlayer = _game.CurrentPlayer;
            if (!(debufedPlayer.Hand.Find(c => c is DebufCard) is DebufCard card1)) throw new ArgumentNullException(nameof(card1));

            _game.ExecuteTurn(new DebufAction(card1, debufedPlayer));
            
            while (_game.CurrentPlayer.Hand.Count(c => c is HealCard hc && hc.Heal != card1.Debuf) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var anotherPlayer = _game.CurrentPlayer;
            var card2 = anotherPlayer.Hand.Find(c => c is HealCard hc && hc.Heal != card1.Debuf) as HealCard;
            
            var turnResult = _game.ExecuteTurn(new HealAction(card2, debufedPlayer));

            Assert.IsInstanceOfType(turnResult, typeof(UnacceptableActionResult));
        }
        
        /// <summary>
        /// Попытка починить поломку не той картой с альтернативой.
        /// </summary>
        [TestMethod]
        public void AlternateHealWrongDebufTest()
        {
            while (_game.CurrentPlayer.Hand.Count(c => c is DebufCard) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var debufedPlayer = _game.CurrentPlayer;
            if (!(debufedPlayer.Hand.Find(c => c is DebufCard) is DebufCard card1)) throw new ArgumentNullException(nameof(card1));

            _game.ExecuteTurn(new DebufAction(card1, debufedPlayer));
            
            while (_game.CurrentPlayer.Hand.Count(c => c is HealAlternativeCard hc && hc.HealAlternative1 != card1.Debuf && hc.HealAlternative2 != card1.Debuf) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var anotherPlayer = _game.CurrentPlayer;
            var card2 = anotherPlayer.Hand.Find(c => c is HealAlternativeCard hc && hc.HealAlternative1 != card1.Debuf && hc.HealAlternative2 != card1.Debuf) as HealAlternativeCard;
            
            var turnResult = _game.ExecuteTurn(new HealAlternativeAction(card2, debufedPlayer));

            Assert.IsInstanceOfType(turnResult, typeof(UnacceptableActionResult));
        }
        
        /// <summary>
        /// Попытка посмотреть золотую жилу, которая уже известна.
        /// </summary>
        [TestMethod]
        public void InvestigateAlreadyInvestigatedTest()
        {
            while (_game.CurrentPlayer.Hand.Count(c => c is InvestigateCard) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var currentPlayer = _game.CurrentPlayer;
            var card = currentPlayer.Hand.Find(c => c is InvestigateCard) as InvestigateCard;

            _game.ExecuteTurn(new InvestigateAction(card, EndVariant.Center)); 
            
            while (_game.CurrentPlayer != currentPlayer)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }
            
            var turnResult = _game.ExecuteTurn(new InvestigateAction(card, EndVariant.Center));

            Assert.IsInstanceOfType(turnResult, typeof(UnacceptableActionResult));
        }
    }
}
