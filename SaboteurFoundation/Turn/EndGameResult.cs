using System;
using System.Collections.Generic;
using System.Text;

namespace SaboteurFoundation.Turn
{
    public class EndGameResult
    {
        public Dictionary<Player, int> WinnersTable { get; }

        public EndGameResult(Dictionary<Player, int> table)
        {
            WinnersTable = table;
        }
    }
}
