using System.Linq;
using SaboteurFoundation;
using SaboteurFoundation.Cards;
using SaboteurFoundation.Turn;

namespace SaboteurTest
{
    /// <summary>
    /// Полезная функциональность при тестировании.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Построить в игре туннель в указанной клетке.
        /// </summary>
        /// <param name="game">Эксемпляр игры.</param>
        /// <param name="x">X-координата на игровом поле.</param>
        /// <param name="y">Y-координата на игровом поле.</param>
        /// <param name="side"></param>
        /// <param name="withOppositeSide"></param>
        /// <returns>Результат хода.</returns>
        /// <remarks>
        /// Постройка осуществляется номральным течением игры - первым попавшимся игроком с подходящей картой.
        /// </remarks>
        public static TurnResult BuildTunnelAt(SaboteurGame game, int x, int y, ConnectorType side, bool withOppositeSide = false)
        {
            return BuildTunnelAtBy(game, x, y, side, withOppositeSide);
        }

        /// <summary>
        /// Построить в игре туннель в указанной клетке конкретным игроком.
        /// </summary>
        /// <param name="game">Эксемпляр игры.</param>
        /// <param name="x">X-координата на игровом поле.</param>
        /// <param name="y">Y-координата на игровом поле.</param>
        /// <param name="side"></param>
        /// <param name="withOppositeSide"></param>
        /// <param name="builder">Игрок, который должен осуществить постройку.</param>
        /// <returns>Результат хода.</returns>
        /// <remarks>
        /// Постройка осуществляется номральным течением игры - первым попавшимся игроком с подходящей картой.
        /// </remarks>
        public static TurnResult BuildTunnelAtBy(SaboteurGame game, int x, int y, ConnectorType side, bool withOppositeSide = false, Player builder = null)
        {          
            var flippedSide = side.Flip();
            // ReSharper disable once PossibleNullReferenceException
            while (builder != null ^ game.CurrentPlayer == builder ^ game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && !tc.IsDeadlock && tc.Outs.Contains(flippedSide) && (!withOppositeSide || tc.Outs.Contains(side))) == 0)
            {
                game.ExecuteTurn(new SkipAction(game.CurrentPlayer.Hand.First()));
            }

            var tunnelCard = game.CurrentPlayer.Hand.Find(c => c is TunnelCard tc && !tc.IsDeadlock && tc.Outs.Contains(flippedSide) && (!withOppositeSide || tc.Outs.Contains(side))) as TunnelCard;

            return game.ExecuteTurn(new BuildAction(tunnelCard, x, y, side));
        }
    }
}