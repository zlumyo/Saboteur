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
            _SwapCard(action.CardToAct);

            TurnResult result;
            switch (action)
            {
                case BuildAction ba:
                    result = _ProcessBuildAction(ba);
                    break;
                case SkipAction sa:
                    result = _ProcessSkipAction(sa);
                    break;
                case PlayInvestigateAction ia:
                    result = _ProcessPlayInvestigateAction(ia);
                    break;
                case PlayDebufAction da:
                    result = _ProcessPlayDebufAction(da);
                    break;
                case PlayBufAction ba:
                    result = _ProcessPlayBufAction(ba);
                    break;
                case PlayBufAlternativeAction baa:
                    result = _ProcessPlayBufAlternativeAction(baa);
                    break;
                case CollapseAction ca:
                    result = _ProcessCollapseAction(ca);
                    break;
                default:
                    result = null;
                    break;
            }

            return result;
        }

        /// <summary>
        /// Performs 'collapse tunnel card' in game.
        /// </summary>
        /// <param name="ca">Parameters of action</param>
        /// <returns>Result of turn.</returns>
        private TurnResult _ProcessCollapseAction(CollapseAction ca)
        {
            // пытаемся найти на поле карту с указанной координатой
            HashSet<(int, int)> watched = new HashSet<(int, int)>();
            var (result, xResult, yResult) = _ScanField(_field.Start, 0, 0, ca.X, ca.Y, watched);
            if (result == null) // если таковой нет, то такой ход недопустим
            {
                return new UnacceptableActionResult();
            }

            result.HasCollapsed = true;

            return new NewTurnResult(_NextPlayer());
        }

        /// <summary>
        /// Performs 'build tunnel card' in game.
        /// </summary>
        /// <param name="ba">Parameters of action</param>
        /// <returns>Result of turn.</returns>
        private TurnResult _ProcessBuildAction(BuildAction ba)
        {
            if (CurrentPlayer.Debufs.Count != 0) // если у игнока есть дебафы, то строить ему нельзя
            {
                return new UnacceptableActionResult();
            }

            TunnelCard tunnelCard = ba.CardToAct as TunnelCard;
            if (tunnelCard.IsDeadlock && WithoutDeadlocks) // если это игра без тупиков, то такой ход недопустим
            {
                return new UnacceptableActionResult();
            }

            // пытаемся найти на поле карту с указанной координатой
            HashSet<(int, int)> watched = new HashSet<(int, int)>();
            var (result, xResult, yResult) = _ScanField(_field.Start, 0, 0, ba.XNear, ba.YNear, watched);
            if (result == null) // если таковой нет, то такой ход недопустим
            {
                return new UnacceptableActionResult();
            }

            // если у найденной карты нет коннектора в искомую сторону, то такой ход недопустим
            if (result.Outs.Count(_out => _out.Type == ba.SideOfNearCard) == 0)
            {
                return new UnacceptableActionResult();
            }

            var connector = result.Outs.First(_out => _out.Type == ba.SideOfNearCard);
            // если нужный коннектор уже соединён с другой картой, то такой ход недопустим
            if (connector.Next != null && !connector.Next.HasCollapsed)
            {
                return new UnacceptableActionResult();
            }

            // если новая карта не подходит к нужному коннектору, то такой ход недопустим
            if (!_CheckConnectors(connector.Type, tunnelCard.Outs, out HashSet<ConnectorType> outs))
            {
                return new UnacceptableActionResult();
            }

            // теперь можно класть карту на поле
            connector.Next = new GameCell(CellType.TUNNEL, outs.Select(cType => new Connector(cType)).ToHashSet());

            // проверяем, достигли ли какой-нибудь финишной карты
            if (_field.CheckFinishReached(connector.Next, xResult+Connector.ConnectorTypeToDeltaX(connector.Type), yResult+Connector.ConnectorTypeToDeltaY(connector.Type), out GameCell[] finishes))
            {
                // переворачиваем финишные карты для всех игроков
                foreach (var finish in finishes)
                {
                    foreach (var player in Players)
                    {
                        var variant = GameField.EndsCoordinates.First((pair => pair.Value.Item1 == xResult && pair.Value.Item2 == yResult)).Key;
                        player.EndsStatuses[variant] = finish.Type == CellType.FAKE ? TargetStatus.FAKE : TargetStatus.REAL;
                    }
                }                

                // если одна из финишных карт - золотая, то заканчиваем раунд
                if (finishes.Any(finish => finish.Type == CellType.GOLD))
                {
                    // если раунд был не последний, то начинаем новый
                    if (Round != ROUNDS_IN_GAME)
                    {
                        // распределяем золото
                        for (int i = 0; i < (Players.Count == 10 ? 9 : Players.Count); i++)
                        {
                            CurrentPlayer.Gold += _popGoldHeap();
                            _NextPlayer();
                            if (CurrentPlayer.Role == GameRole.BAD)
                            {
                                _NextPlayer();
                            }
                        }

                        Round++; // увеличиваем счётчик раундов
                        _PrepareRound(); // подготовливем ирговое поле к новому раунду
                        return new NewRoundResult(CurrentPlayer);
                    }
                    else // иначе заканчиваем игру
                    {
                        IsGameEnded = true;
                        return new EndGameResult(Players.Where(p => p.Gold == Players.Max(x => x.Gold)).ToArray());
                    }
                }
            }

            return new NewTurnResult(_NextPlayer()); // по умолчанию передаётся ход другому игроку
        }

        private bool _CheckConnectors(ConnectorType type, HashSet<ConnectorType> outs, out HashSet<ConnectorType> realOuts)
        {
            // TODO проверить другие карты рядом с остальными коннекторами

            var flippedOuts = FlipOuts();
            var flippedType = FlipConnectorType(type);

            if (outs.Contains(flippedType))
            {
                realOuts = outs;
                return true;
            }
            else if (flippedOuts.Contains(flippedType))
            {
                realOuts = flippedOuts;
                return true;
            }
            else
            {
                realOuts = null;
                return false;
            }

            HashSet<ConnectorType> FlipOuts()
            {
                return outs.Select(FlipConnectorType).ToHashSet();
            }

            ConnectorType FlipConnectorType(ConnectorType cType)
            {
                switch (type)
                {
                    case ConnectorType.DOWN:
                        return ConnectorType.UP;
                    case ConnectorType.LEFT:
                        return ConnectorType.RIGHT;
                    case ConnectorType.RIGHT:
                        return ConnectorType.LEFT;
                    case ConnectorType.UP:
                        return ConnectorType.DOWN;
                    default:
                        return cType;
                }
            }
        }

        private (GameCell, int, int) _ScanField(GameCell current, int xCurrent, int yCurrent, int xTarget, int yTarget, HashSet<(int, int)> watched)
        {
            if (current.HasCollapsed) return (null, 0, 0);

            if (xCurrent == xTarget && yCurrent == yTarget)
            {
                return (current, xCurrent, yCurrent);
            }
            else
            {
                watched.Add((xCurrent, yCurrent));
                var temp = current.Outs.Where(_out => _out.Next != null && !watched.Contains((xTarget+ Connector.ConnectorTypeToDeltaX(_out.Type), yTarget+ Connector.ConnectorTypeToDeltaY(_out.Type))))
                    .Select(filtered => _ScanField(filtered.Next, xTarget+ Connector.ConnectorTypeToDeltaX(filtered.Type), yTarget+ Connector.ConnectorTypeToDeltaY(filtered.Type), xTarget, yTarget, watched))
                    .Where(result => result.Item1 != null).ToArray();
                return temp.Length == 0 ? (null, 0, 0) : temp[0];
            }
        }

        /// <summary>
        /// Performs 'play heal alternative card' in game.
        /// </summary>
        /// <param name="baa">Parameters of action.</param>
        /// <returns>Result of turn.</returns>
        private TurnResult _ProcessPlayBufAlternativeAction(PlayBufAlternativeAction baa)
        {
            TurnResult result;
            var playerToBufAlt = Players.First(x => x == baa.PlayerToBuf);
            var healAltCard = baa.CardToAct as HealAlternativeCard;
            if (playerToBufAlt.Debufs.Contains(healAltCard.HealAlternative1))
            {
                playerToBufAlt.Debufs.Remove(healAltCard.HealAlternative1);
                result = new NewTurnResult(_NextPlayer());
            }
            else if (playerToBufAlt.Debufs.Contains(healAltCard.HealAlternative2))
            {
                playerToBufAlt.Debufs.Remove(healAltCard.HealAlternative2);
                result = new NewTurnResult(_NextPlayer());
            }
            else
            {
                result = new UnacceptableActionResult();
            }
            
            return result;
        }

        /// <summary>
        /// Performs 'play heal card' in game.
        /// </summary>
        /// <param name="ba">Parameters of action.</param>
        /// <returns>Result of turn.</returns>
        private TurnResult _ProcessPlayBufAction(PlayBufAction ba)
        {
            TurnResult result;
            var playerToBuf = Players.First(x => x == ba.PlayerToBuf);
            var healCard = ba.CardToAct as HealCard;
            if (playerToBuf.Debufs.Contains(healCard.Heal))
            {
                playerToBuf.Debufs.Remove(healCard.Heal);
                result = new NewTurnResult(_NextPlayer());
            }
            else
            {
                result = new UnacceptableActionResult();
            }         
            return result;
        }

        /// <summary>
        /// Performs 'play debuf card' in game.
        /// </summary>
        /// <param name="da">Parameters of action.</param>
        /// <returns>Result of turn.</returns>
        private TurnResult _ProcessPlayDebufAction(PlayDebufAction da)
        {
            TurnResult result;
            var playerToDebuf = Players.First(x => x == da.PlayerToDebuf);
            var debufCard = da.CardToAct as DebufCard;
            if (!playerToDebuf.Debufs.Contains(debufCard.Debuf))
            {
                playerToDebuf.Debufs.Add(debufCard.Debuf);
                result = new NewTurnResult(_NextPlayer());
            }
            else
            {
                result = new UnacceptableActionResult();
            }           
            return result;
        }

        /// <summary>
        /// Performs 'play investigate card' in game.
        /// </summary>
        /// <param name="ia">Parameters of action.</param>
        /// <returns>Result of turn.</returns>
        private TurnResult _ProcessPlayInvestigateAction(PlayInvestigateAction ia)
        {
            TurnResult result;
            if (CurrentPlayer.EndsStatuses[ia.Variant] == TargetStatus.UNKNOW)
            {
                CurrentPlayer.EndsStatuses[ia.Variant] = _field.Ends[ia.Variant].Type == CellType.GOLD ? TargetStatus.REAL : TargetStatus.FAKE;
                result = new NewTurnResult(_NextPlayer());
            }
            else
            {
                result = new UnacceptableActionResult();
            }            
            return result;
        }

        /// <summary>
        /// Performs 'skip turn' in game.
        /// </summary>
        /// <param name="sa">Parameters of action.</param>
        /// <returns>Result of turn.</returns>
        private TurnResult _ProcessSkipAction(SkipAction sa)
        {
            TurnResult result;   

            if (_skipedTurnsInLine == Players.Count)
            {
                if (Round != ROUNDS_IN_GAME)
                {
                    var badBoys = Players.Where(p => p.Role == GameRole.BAD).ToArray();
                    switch (badBoys.Length)
                    {
                        case 1:
                            foreach (var p in badBoys)
                            {
                                _popGoldHeapWhile(4);
                                p.Gold += 4;
                            }
                            break;
                        case 2:
                        case 3:
                            foreach (var p in badBoys)
                            {
                                _popGoldHeapWhile(3);
                                p.Gold += 3;
                            }
                            break;
                        case 4:
                            foreach (var p in badBoys)
                            {
                                _popGoldHeapWhile(2);
                                p.Gold += 2;
                            }
                            break;
                        default:
                            break;
                    }

                    Round++;
                    _PrepareRound();                   
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

            return result;
        }

        /// <summary>
        /// Drops card from action and getting next from _deck.
        /// </summary>
        /// <param name="card">Card playing during turn.</param>
        private void _SwapCard(Card card)
        {
            var index = CurrentPlayer.Hand.FindIndex(x => x.Equals(card));
            if (index == -1) throw new ArgumentOutOfRangeException("There is no such card in hand of current player.");
            CurrentPlayer.Hand.RemoveAt(index);
            if (_deck.Count == 0) _skipedTurnsInLine++;
            else CurrentPlayer.Hand.Add(_deck.Pop());
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

        private void _popGoldHeapWhile(int value)
        {
            var init = 0;
            List<int> toRemove = new List<int>();
            foreach (int i in _goldHeap)
            {
                if (init + i > value) continue;
                toRemove.Add(i);
                init += i;              
                if (init == value) break;
            }
            foreach (int i in toRemove) _goldHeap.Remove(i);
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