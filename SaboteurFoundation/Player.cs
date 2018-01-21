using System;
using System.Collections.Generic;

namespace SaboteurFoundation
{
    /// <summary>
    /// Игрок в игре.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Имя.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Количество золота.
        /// </summary>
        public int Gold { get; internal set; }
        /// <summary>
        /// Роль.
        /// </summary>
        public GameRole Role { get; internal set; }
        /// <summary>
        /// Набор карт в руке.
        /// </summary>
        public List<Card> Hand { get; }
        /// <summary>
        /// Сломанные предметы.
        /// </summary>
        public HashSet<Effect> Debufs { get; }
        /// <summary>
        /// Сведения золотых жилах.
        /// </summary>
        public Dictionary<EndVariant, TargetStatus> EndsStatuses { get; }

        /// <summary>
        /// Создаёт игрока с укзанным именем, ролью и набором картв руке.
        /// </summary>
        /// <param name="name">Имя.</param>
        /// <param name="role">Роль.</param>
        /// <param name="hand">Набор карт.</param>
        public Player(string name, GameRole role, IEnumerable<Card> hand)
        {
            Name = name;
            Role = role;
            Gold = 0;
            Hand = new List<Card>(hand);
            Debufs = new HashSet<Effect>();

            EndsStatuses = new Dictionary<EndVariant, TargetStatus>(3);
            foreach (EndVariant endVariant in Enum.GetValues(typeof(EndVariant)))
            {
                EndsStatuses.Add(endVariant, TargetStatus.Unknow);
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
            return !(p1 is null) && p1.Equals(p2);
        }

        public static bool operator !=(Player p1, Player p2)
        {
            return !(p1 is null) && !p1.Equals(p2);
        }

        public void ClearEndStatuses()
        {
            foreach (EndVariant endVariant in Enum.GetValues(typeof(EndVariant)))
            {
                EndsStatuses[endVariant] = TargetStatus.Unknow;
            }
        }
    }
}
