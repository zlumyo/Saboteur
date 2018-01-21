using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SaboteurServer.Interfaces;
using SaboteurServer.Models;

namespace SaboteurServer.Services
{
    /// <summary>
    /// Реализация пула игроков, хранящая данные в оперативной памяти.
    /// </summary>
    public class InMemoryPlayersPool : IPlayersPool
    {
        public string Add(string playerName, SearchParams searchParams)
        {
            throw new NotImplementedException();
        }

        public bool CheckGameReady(string partyId)
        {
            throw new NotImplementedException();
        }

        public string CheckPartyReady(string playerId)
        {
            throw new NotImplementedException();
        }

        public void Remove(string plaerId)
        {
            throw new NotImplementedException();
        }
    }
}
