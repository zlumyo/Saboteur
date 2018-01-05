using Microsoft.VisualStudio.TestTools.UnitTesting;
using SaboteurFoundation;
using System;
using System.Linq;

namespace SaboteurTest
{
    [TestClass]
    public class SaboteurNewGameTest
    {
        private static readonly string[] _minPlayers = { "player1", "player2", "player3" };
        private static readonly string[] _maxPlayers = { "player4", "player5", "player6",
                                                         "player7", "player8", "player9",
                                                         "player10" };
        private const string _tooLessPlayers = "player1,player2";
        private const string _tooMuchPlayers = "player1,player2,player3,player4,player5," +
                                               "player6,player7,player8,player9,player10,player11";

        private readonly SaboteurGame _game;
        private readonly SaboteurGame _gameWithoutDeadlocks;
        private readonly SaboteurGame _gameSkipLoosers;

        public SaboteurNewGameTest()
        {
            _game = SaboteurGame.NewGame(withoutDeadlocks: false, skipLoosers: false, _minPlayers);
            _gameWithoutDeadlocks = SaboteurGame.NewGame(withoutDeadlocks: true, skipLoosers: false, _minPlayers);
            _gameSkipLoosers = SaboteurGame.NewGame(withoutDeadlocks: false, skipLoosers: true, _minPlayers);
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
            CollectionAssert.AreEqual(_minPlayers, _game.Players.Select(p => p.Name).ToArray());
            CollectionAssert.AreEqual(_minPlayers, _gameWithoutDeadlocks.Players.Select(p => p.Name).ToArray());
            CollectionAssert.AreEqual(_minPlayers, _gameSkipLoosers.Players.Select(p => p.Name).ToArray());
        }

        [TestMethod]
        public void CurrentPlayerValue()
        {
            var expected = _minPlayers.Last();
            Assert.AreEqual(expected, _game.CurrentPlayer.Name, "Simple game failed.");
            Assert.AreEqual(expected, _gameWithoutDeadlocks.CurrentPlayer.Name, "WithoutDeadlocks game failed.");
            Assert.AreEqual(expected, _gameSkipLoosers.CurrentPlayer.Name, "SkipLoosers game failed.");
        }

        [DataTestMethod]
        [DataRow(_tooLessPlayers)]
        [DataRow(_tooMuchPlayers)]
        public void OutOfAllowedPlayersCount(string players)
        {
            var splittedPlayers = players.Split(',');
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => SaboteurGame.NewGame(false, false, splittedPlayers),
                $"Players count - {splittedPlayers.Length}"
            );
        }

        [TestMethod]
        public void CheckGoldHeap()
        {
            var groups = _game._goldHeap.GroupBy(x => x).Select(x => (x.Key, x.Sum())).ToDictionary(x => x.Key, y => y.Item2);
            Assert.AreEqual(16, groups[1], "One's is failed");
            Assert.AreEqual(8, groups[2] / 2, "Two's is failed");
            Assert.AreEqual(4, groups[3] / 3, "Three's is failed");
        }
    }
}
