using System.Collections.Generic;
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
        /// <param name="outs">Выходы, которые должны быть у тоннеля</param>
        /// <returns>Результат хода.</returns>
        /// <remarks>
        /// Постройка осуществляется номральным течением игры - первым попавшимся игроком с подходящей картой.
        /// Если <paramref name="outs"/> не задан, то будет построе первый попавшийся туннель-нетупик.
        /// </remarks>
        public static TurnResult BuildTunnelAt(SaboteurGame game, int x, int y, ISet<ConnectorType> outs = null)
        {
            return BuildTunnelAtBy(game, x, y, outs);
        }

        /// <summary>
        /// Построить в игре туннель в указанной клетке конкретным игроком.
        /// </summary>
        /// <param name="game">Эксемпляр игры.</param>
        /// <param name="x">X-координата на игровом поле.</param>
        /// <param name="y">Y-координата на игровом поле.</param>
        /// <param name="outs">Выходы, которые должны быть у тоннеля</param>
        /// <param name="builder">Игрок, который должен осуществить постройку.</param>
        /// <returns>Результат хода.</returns>
        /// <remarks>
        /// Постройка осуществляется номральным течением игры - первым попавшимся игроком с подходящей картой.
        /// Если <paramref name="outs"/> не задан, то будет построе первый попавшийся туннель-нетупик.
        /// </remarks>
        public static TurnResult BuildTunnelAtBy(SaboteurGame game, int x, int y, ISet<ConnectorType> outs = null, Player builder = null)
        {          
            while (builder != null ^ game.CurrentPlayer == builder ^ game.CurrentPlayer.Hand.Count(c => c is TunnelCard tc && !tc.IsDeadlock && (outs == null || tc.Outs.IsSupersetOf(outs))) == 0)
            {
                game.ExecuteTurn(new SkipAction(game.CurrentPlayer.Hand.First()));
            }

            var tunnelCard = game.CurrentPlayer.Hand.Find(c => c is TunnelCard tc && !tc.IsDeadlock && (outs == null || tc.Outs.IsSupersetOf(outs))) as TunnelCard;

            return game.ExecuteTurn(new BuildAction(tunnelCard, x, y));
        }
    }
}