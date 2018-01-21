using System;
using SaboteurServer.Models;

namespace SaboteurServer.Interfaces
{
    /// <summary>
    /// Базовый интерфейс для пула игроков, находящихся в поиске игры.
    /// </summary>
    public interface IPlayersPool
    {
        /// <summary>
        /// Добавить игрока в пул поиска.
        /// </summary>
        /// <param name="playerName">Имя игрока.</param>
        /// <param name="searchParams">Параметры игры, которую ищет игрок.</param>
        /// <returns>ID игрока в пуле.</returns>
        String Add(string playerName, SearchParams searchParams);

        /// <summary>
        /// Убрать игрока из пула поиска.
        /// </summary>
        /// <param name="plaerId">ID игрока.</param>
        void Remove(String plaerId);

        /// <summary>
        /// Проверить, сформированна ли группы для игры.
        /// </summary>
        /// <param name="playerId">ID игрока.</param>
        /// <returns>ID группы в случае готовности, иначе - null.</returns>
        String CheckPartyReady(String playerId);

        /// <summary>
        /// Проверить, готовы ли все игроки группы к игре.
        /// </summary>
        /// <param name="partyId">ID группы.</param>
        /// <returns>Флаг готовности.</returns>
        bool CheckGameReady(String partyId);
    }
}
