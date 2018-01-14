using Microsoft.VisualStudio.TestTools.UnitTesting;
using SaboteurFoundation;
using SaboteurFoundation.Cards;
using SaboteurFoundation.Turn;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SaboteurTest
{
    [TestClass]
    public class SaboteurExecuteTurnTest
    {
        private static readonly string[] _minPlayers = { "player1", "player2", "player3" };

        private readonly SaboteurGame _game;

        public SaboteurExecuteTurnTest()
        {
            _game = SaboteurGame.NewGame(withoutDeadlocks: false, skipLoosers: false, _minPlayers);
        }

        [TestMethod]
        public void SkipTurnTest()
        {
            var currentPlayer = _game.CurrentPlayer;
            var expectedCard = _game._deck.Peek();
            var expectedDeckSize = _game._deck.Count - 1;
            var expectedCardCount = 1 + currentPlayer.Hand.Count(c => c.Equals(expectedCard));

            _game.ExecuteTurn(new SkipAction(currentPlayer.Hand.First()));

            Assert.AreEqual(expectedDeckSize, _game._deck.Count, "New deck's size has failed.");
            Assert.AreEqual(6, currentPlayer.Hand.Count, "New hand's size has failed.");
            Assert.AreEqual(expectedCardCount, currentPlayer.Hand.Count(c => c.Equals(expectedCard)), "New card's count has failed.");
        }
    }
}
