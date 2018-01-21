using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SaboteurServer.Interfaces;
using SaboteurServer.Models;

namespace SaboteurServer.Controllers
{
    [Route("api/[controller]")]
    public class SearchGameController : Controller
    {
        private readonly IPlayersPool _playersPool;

        public SearchGameController(IPlayersPool playersPool)
        {
            _playersPool = playersPool;
        }

        // POST api/searchgame/add/{playerName}
        [HttpPost(template: "{playerName}", Name = "add")]
        public String Add(string playerName, [FromBody]SearchParams @params)
        {
            return _playersPool.Add(playerName, @params);
        }

        // DELETE api/searchgame/remove/{playerId}
        [HttpDelete(template: "{playerId}", Name = "remove")]
        public void Remove(string playerId)
        {
            _playersPool.Remove(playerId);
        }

        // GET api/values/checkparty/{playerId}
        [HttpGet(template: "{playerId}", Name = "checkparty")]
        public string CheckParty(string playerId)
        {
            return _playersPool.CheckPartyReady(playerId);
        }

        // GET api/values/checkgame/{playerId}
        [HttpGet(template: "{partyId}", Name = "checkgame")]
        public bool CheckGame(string partyId)
        {
            return _playersPool.CheckGameReady(partyId);
        }
    }
}
