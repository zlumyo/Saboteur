using System;
using System.Collections.Generic;

namespace SaboteurFoundation
{
    public class Player
    {
        public string Name { get; }
        public int Gold { get; internal set; }
        public GameRole Role { get; internal set; }
        public List<Card> Hand { get; }
        public HashSet<Effect> Debufs { get; }
        public Dictionary<EndVariant, TargetStatus> EndsStatuses { get; }

        public Player(string name, GameRole role, Card[] hand)
        {
            Name = name;
            Role = role;
            Gold = 0;
            Hand = new List<Card>(hand);
            Debufs = new HashSet<Effect>();

            EndsStatuses = new Dictionary<EndVariant, TargetStatus>(3);
            foreach (EndVariant endVariant in Enum.GetValues(typeof(EndVariant)))
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

        public static bool operator ==(Player p1, Player p2)
        {
            return !ReferenceEquals(p1, null) && p1.Equals(p2);
        }

        public static bool operator !=(Player p1, Player p2)
        {
            return !ReferenceEquals(p1, null) && !p1.Equals(p2);
        }

        public void ClearEndStatuses()
        {
            foreach (EndVariant endVariant in Enum.GetValues(typeof(EndVariant)))
            {
                EndsStatuses[endVariant] = TargetStatus.UNKNOW;
            }
        }
    }
}
