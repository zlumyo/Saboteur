using SaboteurFoundation.Cards;
using SaboteurFoundation.Turn;
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
        /// Count of rounds per game.
        /// </summary>
        public const int ROUNDS_IN_GAME = 3;

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
        public int Round { get; private set; }
        /// <summary>
        /// Set of players which are participating in game.
        /// </summary>
        public HashSet<Player> Players { get; private set; }
        /// <summary>
        /// Reference to current player.
        /// </summary>
        public Player CurrentPlayer => _playerEnumerator.Current;
        /// <summary>
        /// Flag that indicates the game has ended.
        /// </summary>
        public bool IsGameEnded { get; private set; }

        /// <summary>
        /// Gold cards.
        /// </summary>
        internal List<int> _goldHeap;
        /// <summary>
        /// Deck of gamecards.
        /// </summary>
        internal Stack<Card> _deck;
        /// <summary>
        /// Gamefield instance.
        /// </summary>
        internal GameField _field;
        /// <summary>
        /// Local random engine.
        /// </summary>
        private readonly Random _rnd;
        /// <summary>
        /// Enumerator of Players' HashSet.
        /// </summary>
        private IEnumerator<Player> _playerEnumerator;
        /// <summary>
        /// Count of skipped turns in round while deck is empty.
        /// </summary>
        private int _skipedTurnsInLine;

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
            _goldHeap = new List<int>(28)
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                2, 2, 2, 2, 2, 2, 2, 2,
                3, 3, 3, 3,
            };
            Round = 1;
            IsGameEnded = false;
            WithoutDeadlocks = withoutDeadlocks;
            SkipLoosers = skipLoosers;
            Players = playersNames.Select(name => new Player(name, GameRole.GOOD, new Card[] { })).ToHashSet();

            _PrepareRound();
        }

        /// <summary>
        /// Prepares state of game to new round.
        /// </summary>
        private void _PrepareRound()
        {
            var playersRoles = _GenerateRoles(Players.Count, _rnd);
            var cardsInHand = _CardsInHandByPlayersCount(Players.Count);

            var lastPlayer = Players.Last();
            _playerEnumerator = Players.GetEnumerator();
            while (lastPlayer != _playerEnumerator.Current) _playerEnumerator.MoveNext();

            _deck = new Stack<Card>(_GenerateDeck(_rnd));
            foreach (var p in Players)
            {
                p.Hand.Clear();
                p.Hand.AddRange(_deck.Take(cardsInHand));
                _deck = new Stack<Card>(_deck.Skip(cardsInHand));
            }

            _skipedTurnsInLine = 0;
            var endVariants = Enum.GetValues(typeof(EndVariant)).Cast<EndVariant>();
            _field = new GameField(endVariants.ElementAt(_rnd.Next(endVariants.Count())));
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

        /// <summary>
        /// Performs an action by current player.
        /// </summary>
        /// <param name="action">Action to exectue in this turn.</param>
        /// <returns>Result of turn.</returns>
        public TurnResult ExecuteTurn(TurnAction action)
        {
            if (IsGameEnded)
                return new EndGameResult(Players.Where(p => p.Gold == Players.Max(x => x.Gold)).ToArray());

            TurnResult result;
            switch (action)
            {
                case SkipAction sa:
                    var index = CurrentPlayer.Hand.FindIndex(x => x.Equals(sa.CardToAct));
                    if (index == -1) throw new ArgumentOutOfRangeException("There is no such card in hand of current player.");
                    CurrentPlayer.Hand.RemoveAt(index);
                    if (_deck.Count == 0) _skipedTurnsInLine++;
                    else CurrentPlayer.Hand.Add(_deck.Pop());

                    if (_skipedTurnsInLine == Players.Count)
                    {
                        if (Round != ROUNDS_IN_GAME)
                        {
                            Round++;
                            _PrepareRound();
                            foreach (var p in Players) p.Gold = _popGoldHeap();
                            result = new NewRoundResult(CurrentPlayer);
                        }
                        else
                        {
                            IsGameEnded = true;
                            result = new EndGameResult(Players.Where(p => p.Gold == Players.Max(x => x.Gold)).ToArray());
                        }
                    }    
                    else
                    {
                        result = new NewTurnResult(_NextPlayer());
                    }  
                    break;
                default:
                    result = null;
                    break;
            }

            return result;
        }

        /// <summary>
        /// Randomly pop the _goldHeap.
        /// </summary>
        /// <returns>Some golden nuggets.</returns>
        private int _popGoldHeap()
        {
            var sumsByCount = _goldHeap.GroupBy(x => x).Select(groups => (groups.Key, groups.Sum(), 0d, 0d)).ToArray();

            var previous = 0d;
            for (var i = 0; i < sumsByCount.Length; i++)
            {
                var (nuggets, count, start, end) = sumsByCount[i];
                var result = Convert.ToDouble(count) / _goldHeap.Count + previous;
                sumsByCount[i] = (nuggets, count, previous, result);
                previous = result;
            }
            var lastEnd = sumsByCount[sumsByCount.Length - 1];
            sumsByCount[sumsByCount.Length - 1] = (lastEnd.Item1, lastEnd.Item2, lastEnd.Item3, 1d);

            var sample = _rnd.NextDouble();
            var (key, _, _ ,_) = sumsByCount.First(quartet => quartet.Item3 <= sample && sample < quartet.Item4);
            _goldHeap.Remove(key);
            return key;
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