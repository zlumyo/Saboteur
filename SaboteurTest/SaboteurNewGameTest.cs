using Microsoft.VisualStudio.TestTools.UnitTesting;
using SaboteurFoundation;
using SaboteurFoundation.Cards;
using System;
using System.Linq;

namespace SaboteurTest
{
    [TestClass]
    public class SaboteurNewGameTest
    {
        private static readonly string[] MinPlayers = { "player1", "player2", "player3" };
        private static readonly string[] MaxPlayers = { "player1", "player2", "player3",
                                                         "player4", "player5", "player6",
                                                         "player7", "player8", "player9",
                                                         "player10" };
        private const string TooLessPlayers = "player1,player2";
        private const string TooMuchPlayers = "player1,player2,player3,player4,player5," +
                                               "player6,player7,player8,player9,player10,player11";

        private readonly SaboteurGame _game;
        private readonly SaboteurGame _gameWithoutDeadlocks;
        private readonly SaboteurGame _gameSkipLoosers;

        public SaboteurNewGameTest()
        {
            _game = SaboteurGame.NewGame(withoutDeadlocks: false, skipLoosers: false, playersNames: MinPlayers);
            _gameWithoutDeadlocks = SaboteurGame.NewGame(withoutDeadlocks: true, skipLoosers: false, playersNames: MinPlayers);
            _gameSkipLoosers = SaboteurGame.NewGame(withoutDeadlocks: false, skipLoosers: true, playersNames: MinPlayers);
        }

        [TestMethod]
        public void WithoutDeadLocksValue()
        {
            Assert.IsFalse(_game.WithoutDeadlocks, "Simple game failed.");
            Assert.IsTrue(_gameWithoutDeadlocks.WithoutDeadlocks, "WithoutDeadlocks game failed.");
            Assert.IsFalse(_gameSkipLoosers.WithoutDeadlocks, "SkipLoosers game failed.");
        }

        [TestMethod]
        public void SkipLoosersValue()
        {
            Assert.IsFalse(_game.SkipLoosers, "Simple game failed.");
            Assert.IsFalse(_gameWithoutDeadlocks.SkipLoosers, "WithoutDeadlocks game failed.");
            Assert.IsTrue(_gameSkipLoosers.SkipLoosers, "SkipLoosers game failed.");
        }

        [TestMethod]
        public void RoundValue()
        {
            Assert.AreEqual(1, _game.Round, "Simple game failed.");
            Assert.AreEqual(1, _gameWithoutDeadlocks.Round, "WithoutDeadlocks game failed.");
            Assert.AreEqual(1, _gameSkipLoosers.Round, "SkipLoosers game failed.");
        }

        [TestMethod]
        public void PlayersList()
        {
            CollectionAssert.AreEqual(MinPlayers, _game.Players.Select(p => p.Name).ToArray());
            CollectionAssert.AreEqual(MinPlayers, _gameWithoutDeadlocks.Players.Select(p => p.Name).ToArray());
            CollectionAssert.AreEqual(MinPlayers, _gameSkipLoosers.Players.Select(p => p.Name).ToArray());
        }

        [TestMethod]
        public void CurrentPlayerValue()
        {
            var expected = MinPlayers.Last();
            Assert.AreEqual(expected, _game.CurrentPlayer.Name, "Simple game failed.");
            Assert.AreEqual(expected, _gameWithoutDeadlocks.CurrentPlayer.Name, "WithoutDeadlocks game failed.");
            Assert.AreEqual(expected, _gameSkipLoosers.CurrentPlayer.Name, "SkipLoosers game failed.");
        }

        [DataTestMethod]
        [DataRow(TooLessPlayers)]
        [DataRow(TooMuchPlayers)]
        public void OutOfAllowedPlayersCount(string players)
        {
            var splittedPlayers = players.Split(',');
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => SaboteurGame.NewGame(false, false, splittedPlayers),
                $"Players count - {splittedPlayers.Length}"
            );
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        public void CheckGoldHeap(int value)
        {
            var groups = _game._goldHeap.GroupBy(x => x).Select(x => (x.Key, x.Sum())).ToDictionary(x => x.Key, y => y.Item2);
            Assert.AreEqual(16 / Convert.ToInt32(Math.Pow(2, value-1)), groups[value] / value, $"{value}'s is failed");
        }

        [TestMethod]
        public void CheckGenerateDeck()
        {
            var rnd = new Random(42);

            var deck = SaboteurGame._GenerateDeck(rnd).ToArray();
            Assert.AreEqual(67, deck.Length, "Total count is failed.");

            var tunnelCount = deck.Count(x => x is TunnelCard);
            var investigateCount = deck.Count(x => x is InvestigateCard);
            var collapseCount = deck.Count(x => x is CollapseCard);
            var healCount = deck.Count(x => x is HealCard);
            var healAlternativeCount = deck.Count(x => x is HealAlternativeCard);
            var debufCount = deck.Count(x => x is DebufCard);

            Assert.AreEqual(40, tunnelCount, "Tunnel count is failed.");
            Assert.AreEqual(6, investigateCount, "Investigate count is failed.");
            Assert.AreEqual(3, collapseCount, "Collapse count is failed.");
            Assert.AreEqual(6, healCount, "Heal count is failed.");
            Assert.AreEqual(3, healAlternativeCount, "HealAlternative count is failed.");
            Assert.AreEqual(9, debufCount, "Debuf count is failed.");
        }

        [TestMethod]
        public void CheckHands()
        {
            Assert.AreEqual(49, _game._deck.Count, "Rest size of deck is failed (6 cards in hand).");
            Assert.AreEqual(18, _game.Players.Sum(p => p.Hand.Count), "Total count of card in hands is failed (6 cards in hand).");

            var middle = SaboteurGame.NewGame(withoutDeadlocks: false, skipLoosers: false, playersNames: MaxPlayers.Take(6).ToArray());
            Assert.AreEqual(37, middle._deck.Count, "Rest size of deck is failed (5 cards in hand).");
            Assert.AreEqual(30, middle.Players.Sum(p => p.Hand.Count), "Total count of card in hands is failed (5 cards in hand).");

            var high = SaboteurGame.NewGame(withoutDeadlocks: false, skipLoosers: false, playersNames: MaxPlayers.Take(8).ToArray());
            Assert.AreEqual(35, high._deck.Count, "Rest size of deck is failed (4 cards in hand).");
            Assert.AreEqual(32, high.Players.Sum(p => p.Hand.Count), "Total count of card in hands is failed (4 cards in hand).");
        }

        [TestMethod]
        public void CheckField()
        {
            Assert.IsTrue(_game._field.Start.Outs.Count == 4 && _game._field.Start.Type == CellType.START, "Start cell is wrong.");
            Assert.IsTrue(_game._field.Ends.Count == 3 && _game._field.Ends.Count(pair => pair.Value.Type == CellType.GOLD) == 1, "End cells are wrong.");
        }

        [TestMethod]
        public void CheckPlayers()
        {
            Assert.IsTrue(_game.Players.All(p => p.EndsStatuses.All(s => s.Value == TargetStatus.UNKNOW) && p.Gold == 0 && p.Debufs.Count == 0), "Players' state is wrong.");
        }
    }
}
