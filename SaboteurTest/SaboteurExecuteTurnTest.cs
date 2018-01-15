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
        private static readonly string[] _minPlayers = { "player1", "player2", "player3" };

        private SaboteurGame _game;

        public SaboteurExecuteTurnTest()
        {
        }

        [TestInitialize]
        public void TestInit()
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
            if (currentPlayer.Hand.First().Equals(expectedCard))
            {
                expectedCardCount--;
            }
            
            _game.ExecuteTurn(new SkipAction(currentPlayer.Hand.First()));

            Assert.AreEqual(expectedDeckSize, _game._deck.Count, "New deck's size has failed.");
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

            Assert.IsTrue(currentPlayer.Debufs.Contains(card.Debuf) && currentPlayer.Debufs.Count == 1, "Debufs' state has failed.");
        }


        [TestMethod]
        public void HealPlayerTest()
        {
            while (_game.CurrentPlayer.Hand.Count(c => c is DebufCard) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var player1 = _game.CurrentPlayer;
            var debufCard = player1.Hand.Find(c => c is DebufCard) as DebufCard;
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
            var debufCard = player1.Hand.Find(c => c is DebufCard) as DebufCard;
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
            while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && tc.Outs.Contains(ConnectorType.LEFT)) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var card = _game.CurrentPlayer.Hand.Find(c => c is TunnelCard) as TunnelCard;

            _game.ExecuteTurn(new BuildAction(card, 0, 0, ConnectorType.RIGHT));

            var rightConnectorOfStart = _game._field.Start.Outs.First(c => c.Type == ConnectorType.RIGHT);
            Assert.IsNotNull(rightConnectorOfStart.Next, "Field's state has failed.");
        }

        [TestMethod]
        public void CollapseTunnelTest()
        {
            while (_game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && tc.Outs.Contains(ConnectorType.LEFT)) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var tunnelCard = _game.CurrentPlayer.Hand.Find(c => c is TunnelCard) as TunnelCard;

            _game.ExecuteTurn(new BuildAction(tunnelCard, 0, 0, ConnectorType.RIGHT));

            while (_game.CurrentPlayer.Hand.Count(c => c is CollapseCard) == 0)
            {
                _game.ExecuteTurn(new SkipAction(_game.CurrentPlayer.Hand.First()));
            }

            var collapseCard = _game.CurrentPlayer.Hand.Find(c => c is CollapseCard) as CollapseCard;

            _game.ExecuteTurn(new CollapseAction(collapseCard, 1, 0));

            var rightConnectorOfStart = _game._field.Start.Outs.First(c => c.Type == ConnectorType.RIGHT);
            Assert.IsTrue(rightConnectorOfStart.Next.HasCollapsed, "Cell's state has failed.");
        }
    }
}
