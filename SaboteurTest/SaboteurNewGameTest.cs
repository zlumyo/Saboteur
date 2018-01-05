using Microsoft.VisualStudio.TestTools.UnitTesting;
using SaboteurFoundation;

namespace SaboteurTest
{
    [TestClass]
    public class SaboteurNewGameTest
    {
        private readonly string[] _minPlayers = { "player1", "player2", "player3" };
        private readonly string[] _maxPlayers = { "player4", "player5", "player6",
                                                  "player7", "player8", "player9",
                                                  "player10" };
        private readonly string[] _tooLessPlayers = { "player1", "player2" };
        private readonly string[] _tooMuchPlayers = { "player4", "player5", "player6",
                                                      "player7", "player8", "player9",
                                                      "player10", "player11" };

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
    }
}
