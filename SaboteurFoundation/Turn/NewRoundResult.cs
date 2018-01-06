using System;
using System.Collections.Generic;
using System.Text;

namespace SaboteurFoundation.Turn
{
    public class NewRoundResult : TurnResult
    {
        public Player FristPlayer { get; }

        public NewRoundResult(Player first)
        {
            FristPlayer = first;
        }
    }
}
