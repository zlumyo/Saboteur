using System;
using System.Collections.Generic;
using System.Text;

namespace SaboteurFoundation.Turn
{
    public class EndGameResult : TurnResult
    {
        public Player[] Winners { get; }

        public EndGameResult(Player[] winners)
        {
            Winners = winners;
        }
    }
}
