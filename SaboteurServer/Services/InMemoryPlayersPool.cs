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
        private Dictionary<(int, bool, bool), Queue<(Guid, string)>> pools = new Dictionary<(int, bool, bool), Queue<(Guid, string)>>
        {
            { (3, false, false), new Queue<(Guid, string)>() },
            { (4, false, false), new Queue<(Guid, string)>() },
            { (5, false, false), new Queue<(Guid, string)>() },
            { (6, false, false), new Queue<(Guid, string)>() },
            { (7, false, false), new Queue<(Guid, string)>() },
            { (8, false, false), new Queue<(Guid, string)>() },
            { (9, false, false), new Queue<(Guid, string)>() },
            { (10, false, false), new Queue<(Guid, string)>() },
            { (3, true, false), new Queue<(Guid, string)>() },
            { (4, true, false), new Queue<(Guid, string)>() },
            { (5, true, false), new Queue<(Guid, string)>() },
            { (6, true, false), new Queue<(Guid, string)>() },
            { (7, true, false), new Queue<(Guid, string)>() },
            { (8, true, false), new Queue<(Guid, string)>() },
            { (9, true, false), new Queue<(Guid, string)>() },
            { (10, true, false), new Queue<(Guid, string)>() },
            { (3, false, true), new Queue<(Guid, string)>() },
            { (4, false, true), new Queue<(Guid, string)>() },
            { (5, false, true), new Queue<(Guid, string)>() },
            { (6, false, true), new Queue<(Guid, string)>() },
            { (7, false, true), new Queue<(Guid, string)>() },
            { (8, false, true), new Queue<(Guid, string)>() },
            { (9, false, true), new Queue<(Guid, string)>() },
            { (10, false, true), new Queue<(Guid, string)>() },
            { (3, true, true), new Queue<(Guid, string)>() },
            { (4, true, true), new Queue<(Guid, string)>() },
            { (5, true, true), new Queue<(Guid, string)>() },
            { (6, true, true), new Queue<(Guid, string)>() },
            { (7, true, true), new Queue<(Guid, string)>() },
            { (8, true, true), new Queue<(Guid, string)>() },
            { (9, true, true), new Queue<(Guid, string)>() },
            { (10, true, true), new Queue<(Guid, string)>() }
        };

        public string Add(string playerName, SearchParams searchParams)
        {
            var keysToAdd = new HashSet<(int, bool, bool)>
            {
                (3, false, false),  (3, true, false),
                (4, false, false),  (4, true, false),
                (5, false, false),  (5, true, false),
                (6, false, false),  (6, true, false),
                (7, false, false),  (7, true, false),
                (8, false, false),  (8, true, false),
                (9, false, false),  (9, true, false),
                (10, false, false), (10, true, false),

                (3, false, true),   (3, true, true),
                (4, false, true),   (4, true, true),
                (5, false, true),   (5, true, true),
                (6, false, true),   (6, true, true),
                (7, false, true),   (7, true, true),
                (8, false, true),   (8, true, true),
                (9, false, true),   (9, true, true),
                (10, false, true),  (10, true, true)
            };

            if (searchParams.PartySize.HasValue)
            {
                keysToAdd.RemoveWhere(triple => triple.Item1 != searchParams.PartySize);
            }

            if (searchParams.WithoutDeadlocks.HasValue)
            {
                keysToAdd.RemoveWhere(triple => triple.Item2 != searchParams.WithoutDeadlocks);
            }

            if (searchParams.SkipLoosers.HasValue)
            {
                keysToAdd.RemoveWhere(triple => triple.Item3 != searchParams.SkipLoosers);
            }

            var guid = Guid.NewGuid();
            foreach (var key in keysToAdd)
            {                
                pools[key].Enqueue((guid, playerName));
            }

            return guid.ToString();
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
