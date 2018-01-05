using SaboteurFoundation.Cards;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SaboteurFoundation
{
    /// <summary>
    /// Stores state of saboteur-game and provides operations to mutate once.
    /// </summary>
    public class SaboteurGame
    {
        /// <summary>
        /// Minimum count of players in game.
        /// </summary>
        public const int MIN_PLAYERS_COUNT = 3;
        /// <summary>
        /// Maximum count of players in game.
        /// </summary>
        public const int MAX_PLAYERS_COUNT = 10;

        /// <summary>
        /// Flag of additional rule which only allows to expand tunnel.
        /// </summary>
        public bool WithoutDeadlocks { get; }
        /// <summary>
        /// Flag of additional rule which bans grabbing gold by broken players.
        /// </summary>
        public bool SkipLoosers { get; }
        /// <summary>
        /// Number of round.
        /// </summary>
        public int Round { get; }
        /// <summary>
        /// Set of players which are participating in game.
        /// </summary>
        public HashSet<Player> Players { get; }
        /// <summary>
        /// Reference to current player.
        /// </summary>
        public Player CurrentPlayer => _playerEnumerator.Current;

        /// <summary>
        /// Gold cards.
        /// </summary>
        internal int[] _goldHeap;
        /// <summary>
        /// Deck of gamecards.
        /// </summary>
        internal Stack<Card> _deck;
        /// <summary>
        /// Gamefield instance.
        /// </summary>
        internal GameField _Field { get; }
        /// <summary>
        /// Local random engine.
        /// </summary>
        private readonly Random _rnd;
        /// <summary>
        /// Enumerator of Players' HashSet.
        /// </summary>
        private IEnumerator<Player> _playerEnumerator;

        /// <summary>
        /// Initializes game with zero-state.
        /// </summary>
        /// <param name="withoutDeadlocks">Ban deadlocks building?</param>
        /// <param name="skipLoosers">Ban broken players to grab a gold?</param>
        /// <param name="playersNames">Set of players.</param>
        /// <param name="rnd">Optional custom random engine.</param>
        /// <remarks>
        /// Player which got cards last will turn first.
        /// </remarks>
        private SaboteurGame(bool withoutDeadlocks, bool skipLoosers, HashSet<string> playersNames, Random rnd)
        {
            _rnd = rnd;
            var playersRoles = _GenerateRoles(playersNames.Count, _rnd);
            _deck = new Stack<Card>(_GenerateDeck(_rnd));
            _goldHeap = new int[28]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                2, 2, 2, 2, 2, 2, 2, 2,
                3, 3, 3, 3,
            };
            var cardsInHand = _CardsInHandByPlayersCount(playersNames.Count);

            WithoutDeadlocks = withoutDeadlocks;
            SkipLoosers = skipLoosers;
            var temp = playersNames.Zip(playersRoles, (name, role) => (name, role)).Select(pair => {
                var result = new Player(pair.name, pair.role, _deck.Take(cardsInHand).ToArray());
                _deck = new Stack<Card>(_deck.Skip(cardsInHand));
                return result;
            });
            var lastPlayer = temp.Last();
            Players = temp.ToHashSet();
            _playerEnumerator = Players.GetEnumerator();
            while (lastPlayer != _playerEnumerator.Current) _playerEnumerator.MoveNext();
            Round = 1;

            var endVariants = Enum.GetValues(typeof(EndVariant)).Cast<EndVariant>();
            _Field = new GameField(endVariants.ElementAt(_rnd.Next(endVariants.Count())));
        }

        /// <summary>
        /// Creates new game.
        /// </summary>
        /// <param name="withoutDeadlocks">Ban deadlocks building?</param>
        /// <param name="skipLoosers">Ban broken players to grab a gold?</param>
        /// <param name="playersNames">Set of players.</param>
        /// <returns>Instance of new game.</returns>
        public static SaboteurGame NewGame(bool withoutDeadlocks, bool skipLoosers, string[] playersNames, Random rnd = null)
        {
            if (playersNames.Length < MIN_PLAYERS_COUNT || playersNames.Length > MAX_PLAYERS_COUNT)
                throw new ArgumentOutOfRangeException($"Players count must be between {MIN_PLAYERS_COUNT} and {MAX_PLAYERS_COUNT}.");

            return new SaboteurGame(withoutDeadlocks, skipLoosers, playersNames.ToHashSet(), rnd ?? new Random());
        }

        public Player ExecuteTurn()
        {
            return _NextPlayer();
        }

        /// <summary>
        /// Generates random sequence of roles of limited count.
        /// </summary>
        /// <param name="rolesCount">Limit of sequence.</param>
        /// <param name="rnd">Random engine.</param>
        /// <returns>Sequence of roles.</returns>
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

        /// <summary>
        /// Generates random sequence of gamecards.
        /// </summary>
        /// <param name="rnd">Random engine.</param>
        /// <returns>Sequence of gamecards.</returns>
        internal static IEnumerable<Card> _GenerateDeck(Random rnd)
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
                else allCards[index] = quertet;
            }

            yield break;
        }

        /// <summary>
        /// Calculates intervals of probability for gamecrads.
        /// </summary>
        /// <param name="data">List of quartes: Card, count of this Card, start and end of interval.</param>
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
            data[data.Count - 1] = (lastEnd.Item1, lastEnd.Item2, lastEnd.Item3, 1d);
        }

        /// <summary>
        /// Map players count to count of cards in their hands.
        /// </summary>
        /// <param name="playersCount"></param>
        /// <returns>Count of cards.</returns>
        private static int _CardsInHandByPlayersCount(int playersCount)
        {
            if (playersCount >= 3 && playersCount <= 5) return 6;
            else if (playersCount >= 6 && playersCount <= 7) return 5;
            else return 4;
        }

        /// <summary>
        /// Sets active next player by round.
        /// </summary>
        /// <returns>New active player.</returns>
        private Player _NextPlayer()
        {
            if (!_playerEnumerator.MoveNext())
            {
                _playerEnumerator.Reset();
            }

            return CurrentPlayer;
        }
    }
}