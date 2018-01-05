using Microsoft.VisualStudio.TestTools.UnitTesting;
using SaboteurFoundation;
using System;

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
    }
}
