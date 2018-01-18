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
        public const int MinPlayersCount = 3;
        /// <summary>
        /// Maximum count of players in game.
        /// </summary>
        public const int MaxPlayersCount = 10;
        /// <summary>
        /// Count of rounds per game.
        /// </summary>
        public const int RoundsInGame = 3;

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
        public HashSet<Player> Players { get; }
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
        internal readonly List<int> GoldHeap;
        /// <summary>
        /// Deck of gamecards.
        /// </summary>
        internal Stack<Card> Deck;
        /// <summary>
        /// Gamefield instance.
        /// </summary>
        internal GameField Field;
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
        /// <remarks>
        /// Player which got cards last will turn first.
        /// </remarks>
        private SaboteurGame(bool withoutDeadlocks, bool skipLoosers, ISet<string> playersNames)
        {
            _rnd = new Random();
            GoldHeap = new List<int>(28)
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                2, 2, 2, 2, 2, 2, 2, 2,
                3, 3, 3, 3
            };
            Round = 1;
            IsGameEnded = false;
            WithoutDeadlocks = withoutDeadlocks;
            SkipLoosers = skipLoosers;
            Players = playersNames.Select(name => new Player(name, GameRole.Good, new Card[] { })).ToHashSet();

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

            Deck = new Stack<Card>(_GenerateDeck(_rnd));
            foreach (var (p, r) in Players.Zip(playersRoles, (p, r) => (p, r)))
            {
                p.Hand.Clear();
                p.ClearEndStatuses();
                p.Role = r;
                p.Hand.AddRange(Deck.Take(cardsInHand));
                Deck = new Stack<Card>(Deck.Skip(cardsInHand));
            }

            _skipedTurnsInLine = 0;
            var endVariants = Enum.GetValues(typeof(EndVariant)).Cast<EndVariant>().ToArray();
            Field = new GameField(endVariants.ElementAt(_rnd.Next(endVariants.Length)));
        }

        /// <summary>
        /// Creates new game.
        /// </summary>
        /// <param name="withoutDeadlocks">Ban deadlocks building?</param>
        /// <param name="skipLoosers">Ban broken players to grab a gold?</param>
        /// <param name="playersNames">Set of players.</param>
        /// <returns>Instance of new game.</returns>
        public static SaboteurGame NewGame(bool withoutDeadlocks, bool skipLoosers, string[] playersNames)
        {
            if (playersNames.Length < MinPlayersCount || playersNames.Length > MaxPlayersCount)
                throw new ArgumentOutOfRangeException($"Players count must be between {MinPlayersCount} and {MaxPlayersCount}.");

            return new SaboteurGame(withoutDeadlocks, skipLoosers, playersNames.ToHashSet());
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
            if (!_SwapCard(action.CardToAct))
                return new UnacceptableActionResult();

            TurnResult result;
            switch (action)
            {
                case BuildAction ba:
                    result = _ProcessBuildAction(ba);
                    break;
                case SkipAction _:
                    result = _ProcessSkipAction();
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
            // Вход в шахту разрушать нельзя.
            if (ca.X == 0 && ca.Y == 0)
                return new UnacceptableActionResult();

            // пытаемся найти на поле карту с указанной координатой
            var result = Field.Scan(ca.X, ca.Y);
            if (result == null) // если таковой нет, то такой ход недопустим
                return new UnacceptableActionResult();

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
            if (CurrentPlayer.Debufs.Count != 0) // если у игрока есть дебафы, то строить ему нельзя
                return new UnacceptableActionResult();

            var tunnelCard = ba.CardToAct as TunnelCard;
            // если это игра без тупиков, то такой ход недопустим
            // ReSharper disable once PossibleNullReferenceException
            if (tunnelCard.IsDeadlock && WithoutDeadlocks)
                return new UnacceptableActionResult();

            // пытаемся найти на поле карту с указанной координатой
            var result = Field.Scan(ba.XNear, ba.YNear);
            if (result == null) // если таковой нет, то такой ход недопустим
                return new UnacceptableActionResult();

            // если у найденной карты нет коннектора в искомую сторону, то такой ход недопустим
            if (!result.Outs.ContainsKey(ba.SideOfNearCard))
                return new UnacceptableActionResult();

            // если нужный коннектор уже соединён с другой картой, то такой ход недопустим
            if (!result.Outs.GetValueOrDefault(ba.SideOfNearCard)?.HasCollapsed ?? false)
                return new UnacceptableActionResult();

            // если новая карта не подходит к нужному коннектору, то такой ход недопустим
            if (!_CheckConnectors(ba.SideOfNearCard, tunnelCard.Outs, result.X, result.Y, out var outs))
                return new UnacceptableActionResult();
         
            // теперь можно класть карту на поле   
            var nextX = result.X + ba.SideOfNearCard.ToDeltaX();
            var nextY = result.Y + ba.SideOfNearCard.ToDeltaY();
            var newCell = Field.PutNewTunnel(nextX, nextY, outs, tunnelCard.IsDeadlock);
            
            // если ещё не достигли финиша, то передаём ход следующем игроку
            if (!Field.CheckFinishReached(newCell, out var finishes))
                return new NewTurnResult(_NextPlayer()); // по умолчанию передаётся ход другому игроку
                             
            // переворачиваем финишные карты для всех игроков
            foreach (var finish in finishes)
            {
                Field.ConnectFinish(finish); // и соединяем финиш с соседями
                
                foreach (var player in Players)
                {
                    var variant = GameField.EndsCoordinates.First(pair => pair.Value.Item1 == finish.X && pair.Value.Item2 == finish.Y).Key;
                    player.EndsStatuses[variant] = finish.Type == CellType.Fake ? TargetStatus.Fake : TargetStatus.Real;
                }
            }                

            // если нет реального золота, то передаём ход следующем игроку
            if (finishes.All(finish => finish.Type != CellType.Gold))
                return new NewTurnResult(_NextPlayer());
            
            // если раунд был не последний, то начинаем новый
            if (Round != RoundsInGame)
            {
                // распределяем золото
                for (var i = 0; i < (Players.Count == 10 ? 9 : Players.Count); i++)
                {
                    CurrentPlayer.Gold += _popGoldHeap();
                    _NextPlayer();
                    if (CurrentPlayer.Role == GameRole.Bad || (SkipLoosers && CurrentPlayer.Debufs.Count != 0))
                        _NextPlayer();
                }

                Round++; // увеличиваем счётчик раундов
                _PrepareRound(); // подготовливем ирговое поле к новому раунду
                return new NewRoundResult(CurrentPlayer);
            }

            // в конце концов заканчиваем игру
            IsGameEnded = true;
            return new EndGameResult(Players.Where(p => p.Gold == Players.Max(x => x.Gold)).ToArray());
        }

        private bool _CheckConnectors(ConnectorType type, HashSet<ConnectorType> outs, int x, int y,
            out HashSet<ConnectorType> realOuts)
        {
            var flippedOuts = FlipOuts();
            var flippedType = type.Flip();

            if (outs.Contains(flippedType))
            {
                realOuts = outs;
                return CheckNeighbors(realOuts);
            }

            if (flippedOuts.Contains(flippedType))
            {
                realOuts = flippedOuts;
                return CheckNeighbors(realOuts);
            }

            realOuts = null;
            return false;

            HashSet<ConnectorType> FlipOuts() => outs.Select(ct => ct.Flip()).ToHashSet();

            // ReSharper disable once ParameterTypeCanBeEnumerable.Local
            bool CheckNeighbors(HashSet<ConnectorType> cTypes)
            {
                return cTypes.Where(_out => _out != flippedType).All(_out => {
                    var cell = Field.Scan(x + _out.ToDeltaX(), y + _out.ToDeltaY());
                    return cell == null || cell.Outs.Count(cellOut => cellOut.Key == _out.Flip()) == 1;
                });
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
            if (healAltCard != null && playerToBufAlt.Debufs.Contains(healAltCard.HealAlternative1))
            {
                playerToBufAlt.Debufs.Remove(healAltCard.HealAlternative1);
                result = new NewTurnResult(_NextPlayer());
            }
            else if (healAltCard != null && playerToBufAlt.Debufs.Contains(healAltCard.HealAlternative2))
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
            if (ba.CardToAct is HealCard healCard && playerToBuf.Debufs.Contains(healCard.Heal))
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
            if (da.CardToAct is DebufCard debufCard && !playerToDebuf.Debufs.Contains(debufCard.Debuf))
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
            if (CurrentPlayer.EndsStatuses[ia.Variant] == TargetStatus.Unknow)
            {
                CurrentPlayer.EndsStatuses[ia.Variant] = Field.Ends[ia.Variant].Type == CellType.Gold ? TargetStatus.Real : TargetStatus.Fake;
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
        /// <returns>Result of turn.</returns>
        private TurnResult _ProcessSkipAction()
        {
            TurnResult result;   
            
            if (_skipedTurnsInLine == Players.Count)
            {
                if (Round != RoundsInGame)
                {
                    var badBoys = Players.Where(p => p.Role == GameRole.Bad).ToArray();
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
                        default:
                            foreach (var p in badBoys)
                            {
                                _popGoldHeapWhile(2);
                                p.Gold += 2;
                            }
                            
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
        private bool _SwapCard(Card card)
        {
            var index = CurrentPlayer.Hand.FindIndex(x => x.Equals(card));
            if (index == -1) return false;
            CurrentPlayer.Hand.RemoveAt(index);
            if (Deck.Count == 0) _skipedTurnsInLine++;
            else CurrentPlayer.Hand.Add(Deck.Pop());
            return true;
        }

        /// <summary>
        /// Randomly pop the _goldHeap.
        /// </summary>
        /// <returns>Some golden nuggets.</returns>
        private int _popGoldHeap()
        {
            var sumsByCount = GoldHeap.GroupBy(x => x).Select(groups => (groups.Key, groups.Sum(), 0d, 0d)).ToArray();

            var previous = 0d;
            for (var i = 0; i < sumsByCount.Length; i++)
            {
                var (nuggets, count, _, _) = sumsByCount[i];
                var result = Convert.ToDouble(count) / GoldHeap.Count + previous;
                sumsByCount[i] = (nuggets, count, previous, result);
                previous = result;
            }
            var lastEnd = sumsByCount[sumsByCount.Length - 1];
            sumsByCount[sumsByCount.Length - 1] = (lastEnd.Item1, lastEnd.Item2, lastEnd.Item3, 1d);

            var sample = _rnd.NextDouble();
            var (key, _, _ ,_) = sumsByCount.First(quartet => quartet.Item3 <= sample && sample < quartet.Item4);
            GoldHeap.Remove(key);
            return key;
        }

        private void _popGoldHeapWhile(int value)
        {
            var init = 0;
            var toRemove = new List<int>();
            foreach (var i in GoldHeap)
            {
                if (init + i > value) continue;
                toRemove.Add(i);
                init += i;              
                if (init == value) break;
            }
            foreach (int i in toRemove) GoldHeap.Remove(i);
        }

        /// <summary>
        /// Generates random sequence of roles of limited count.
        /// </summary>
        /// <param name="rolesCount">Limit of sequence.</param>
        /// <param name="rnd">Random engine.</param>
        /// <returns>Sequence of roles.</returns>
        private static IEnumerable<GameRole> _GenerateRoles(int rolesCount, Random rnd)
        {
            var playersCountToRolesCount = new Dictionary<int, (int, int)>(8)
            {
                { MinPlayersCount, (1, 3) },
                { 4, (1, 4) },
                { 5, (2, 4) },
                { 6, (2, 5) },
                { 7, (3, 5) },
                { 8, (3, 6) },
                { 9, (3, 7) },
                { MaxPlayersCount, (4, 7) }
            };

            var (bads, goods) = playersCountToRolesCount[rolesCount];

            while (bads + goods != 1)
            {
                var total = bads + goods;
                var badChance = Convert.ToDouble(bads) / total;
                var answer = rnd.NextDouble() < badChance ? GameRole.Bad : GameRole.Good;
                yield return answer;
                if (answer == GameRole.Bad) --bads; else --goods;
            }
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
                (HealCard.FromEffect(Effect.Truck), 2, 0, 0),
                (HealCard.FromEffect(Effect.Pick), 2, 0, 0),
                (HealCard.FromEffect(Effect.Lamp), 2, 0, 0),
                (HealAlternativeCard.FromEffect(Effect.Pick, Effect.Truck), 1, 0, 0),
                (HealAlternativeCard.FromEffect(Effect.Pick, Effect.Lamp), 1, 0, 0),
                (HealAlternativeCard.FromEffect(Effect.Lamp, Effect.Truck), 1, 0, 0),
                (DebufCard.FromEffect(Effect.Lamp), 3, 0, 0),
                (DebufCard.FromEffect(Effect.Pick), 3, 0, 0),
                (DebufCard.FromEffect(Effect.Truck), 3, 0, 0)
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
        }

        /// <summary>
        /// Calculates intervals of probability for gamecrads.
        /// </summary>
        /// <param name="data">List of quartes: Card, count of this Card, start and end of interval.</param>
        private static void _CountIntervals(IList<(Card, int, double, double)> data)
        {
            var totalCount = data.Sum(quartet => quartet.Item2);
            var previous = 0d;
            for (var i = 0; i < data.Count; i++)
            {
                var (card, count, _, _) = data[i];
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
            if (playersCount >= 6 && playersCount <= 7) return 5;
            return 4;
        }

        /// <summary>
        /// Sets active next player by round.
        /// </summary>
        /// <returns>New active player.</returns>
        private Player _NextPlayer()
        {
            if (_playerEnumerator.MoveNext()) return CurrentPlayer;
            
            _playerEnumerator.Reset();
            _playerEnumerator.MoveNext();

            return CurrentPlayer;
        }
    }
}