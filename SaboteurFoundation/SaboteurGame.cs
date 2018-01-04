using SaboteurFoundation.Cards;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SaboteurFoundation
{
    public class SaboteurGame
    {
        public const int MIN_PLAYERS_COUNT = 3;
        public const int MAX_PLAYERS_COUNT = 10;

        bool WithoutDeadlocks { get; }
        bool SkipLoosers { get; }
        int Round { get; }
        Player[] Players { get; }

        private int[] _goldHeap;
        private Stack<Card> _deck;
        private GameField _Field { get; }

        private SaboteurGame(bool withoutDeadlocks, bool skipLoosers, string[] playersNames)
        {
            var rnd = new Random();
            var playersRoles = _GenerateRoles(playersNames.Length, rnd);
            _deck = new Stack<Card>(_GenerateDeck(rnd));
            _goldHeap = new int[28]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                2, 2, 2, 2, 2, 2, 2, 2,
                3, 3, 3, 3,
            };
            var cardsInHand = _CardsInHandByPlayersCount(playersNames.Length);

            WithoutDeadlocks = withoutDeadlocks;
            SkipLoosers = skipLoosers;
            playersNames.Zip(playersRoles, (name, role) => (name, role)).Select(pair => {
                var result = new Player(pair.Item1, pair.Item2, _deck.Take(cardsInHand).ToArray());
                _deck = new Stack<Card>(_deck.Skip(cardsInHand));
                return result;
            });
            Round = 1;

            var endVariants = Enum.GetValues(typeof(EndVariant)).Cast<EndVariant>();
            _Field = new GameField(endVariants.ElementAt(rnd.Next(endVariants.Count())));
        }

        public static SaboteurGame NewGame(bool withoutDeadlocks, bool skipLoosers, string[] playersNames)
        {
            if (playersNames.Length < 3 || playersNames.Length > 10)
                throw new ArgumentOutOfRangeException($"Players count must be between {MIN_PLAYERS_COUNT} and {MAX_PLAYERS_COUNT}.");

            return new SaboteurGame(withoutDeadlocks, skipLoosers, playersNames);
        }

        private static IEnumerable<GameRole> _GenerateRoles(int rolesCount, Random rnd)
        {
            var _playersCountToRolesCount = new Dictionary<int, (int, int)>(8)
            {
                { MIN_PLAYERS_COUNT, (1, 3) },
                { 4, (1, 4) },
                { 5, (2, 4) },
                { 6, (2, 5) },
                { 7, (3, 5) },
                { 8, (3, 6) },
                { 9, (3, 7) },
                { MAX_PLAYERS_COUNT, (4, 7) }
            };

            var (bads, goods) = _playersCountToRolesCount[rolesCount];

            while (bads + goods != 1)
            {
                var total = bads + goods;
                var badChance = Convert.ToDouble(bads) / total;
                var answer = rnd.NextDouble() < badChance ? GameRole.BAD : GameRole.GOOD;
                yield return answer;
                if (answer == GameRole.BAD) --bads; else --goods;
            }

            yield break;
        }

        private static IEnumerable<Card> _GenerateDeck(Random rnd)
        {
            var allCardsCount = 67;
            var allCards = new List<(Card, int, double, double)>(allCardsCount)
            {
                (TunnelCard.FromOuts(right: true, left: true), 4, 0, 0),
                (TunnelCard.FromOuts(up: true, right: true, left: true), 5, 0, 0),
                (TunnelCard.FromOuts(up: true, right: true, down: true, left: true), 5, 0, 0),
                (TunnelCard.FromOuts(up: true, right: true), 4, 0, 0),
                (TunnelCard.FromOuts(down: true, right: true), 5, 0, 0),
                (TunnelCard.FromOuts(right: true, isDeadLock: true), 1, 0, 0),
                (TunnelCard.FromOuts(right: true, down: true, left: true, isDeadLock: true), 1, 0, 0),
                (TunnelCard.FromOuts(up: true, right: true, down: true, left: true, isDeadLock: true), 1, 0, 0),
                (TunnelCard.FromOuts(up: true, right: true, isDeadLock: true), 1, 0, 0),
                (TunnelCard.FromOuts(right: true, down: true, isDeadLock: true), 1, 0, 0),
                (TunnelCard.FromOuts(down: true, isDeadLock: true), 1, 0, 0),
                (TunnelCard.FromOuts(up:true, down: true, left: true), 5, 0, 0),
                (TunnelCard.FromOuts(up:true, down: true), 3, 0, 0),
                (TunnelCard.FromOuts(right: true, left: true, isDeadLock: true), 1, 0, 0),
                (TunnelCard.FromOuts(up:true, down: true, left: true, isDeadLock: true), 1, 0, 0),
                (TunnelCard.FromOuts(up:true, down: true, isDeadLock: true), 1, 0, 0),
                (new InvestigateCard(), 6, 0, 0),
                (new CollapseCard(), 3, 0, 0),
                (HealCard.FromEffect(Effect.TRUCK), 2, 0, 0),
                (HealCard.FromEffect(Effect.PICK), 2, 0, 0),
                (HealCard.FromEffect(Effect.LAMP), 2, 0, 0),
                (HealAlternativeCard.FromEffect(Effect.PICK, Effect.TRUCK), 1, 0, 0),
                (HealAlternativeCard.FromEffect(Effect.PICK, Effect.LAMP), 1, 0, 0),
                (HealAlternativeCard.FromEffect(Effect.LAMP, Effect.TRUCK), 1, 0, 0),
                (DebufCard.FromEffect(Effect.LAMP), 3, 0, 0),
                (DebufCard.FromEffect(Effect.PICK), 3, 0, 0),
                (DebufCard.FromEffect(Effect.TRUCK), 3, 0, 0)
            };

            while (allCardsCount != 0)
            {
                _CountIntervals(allCards);
                var sample = rnd.NextDouble();
                var index = allCards.FindIndex(quartet => quartet.Item3 <= sample && sample < quartet.Item4);
                var quertet = allCards[index];
                yield return quertet.Item1;

                quertet.Item2--;
                allCardsCount--;
                if (quertet.Item2 == 0) allCards.RemoveAt(index);
            }

            yield break;
        }

        private static void _CountIntervals(List<(Card, int, double, double)> data)
        {
            var totalCount = data.Sum(quartet => quartet.Item2);
            var previous = 0d;
            for (var i = 0; i < data.Count; i++)
            {
                var (card, count, start, end) = data[i];
                var result = Convert.ToDouble(count) / totalCount + previous;
                data[i] = (card, count, previous, result);
                previous = result;
            }
            var lastEnd = data[data.Count - 1];
            lastEnd.Item4 = 1d;
        }

        private static int _CardsInHandByPlayersCount(int playersCount)
        {
            if (playersCount >= 3 && playersCount <= 5) return 6;
            else if (playersCount >= 6 && playersCount <= 7) return 5;
            else return 4;
        }
    }
}