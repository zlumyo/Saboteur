using System;
using System.Collections.Generic;

namespace SaboteurFoundation
{
    public class Player
    {
        string Name { get; }
        int Gold { get; }
        GameRole Role { get; }
        Card[] Hand { get; }
        HashSet<Effect> Debufs { get; }

        Dictionary<EndVariant, TargetStatus> EndsStatuses { get; }

        public Player(string name, GameRole role, Card[] hand)
        {
            Name = name;
            Role = role;
            Gold = 0;
            Hand = hand;
            Debufs = new HashSet<Effect>();

            EndsStatuses = new Dictionary<EndVariant, TargetStatus>(3);
            foreach (EndVariant endVariant in Enum.GetValues(typeof(ConnectorType)))
            {
                EndsStatuses.Add(endVariant, TargetStatus.UNKNOW);
            }
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is Player p && p.Name == Name;
        }
    }
}
