using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SaboteurServer.Models
{
    /// <summary>
    /// Параметры поиска игры.
    /// </summary>
    public class SearchParams
    {
        /// <summary>
        /// Желаемый количество игроков за столом.
        /// </summary>
        public int? PartySize { get; set; }
        /// <summary>
        /// Без строительства тупиков?
        /// </summary>
        public bool? WithoutDeadlocks { get; set; }
        /// <summary>
        /// Игроки с поломанными инструментами не получают золото?
        /// </summary>
        public bool? SkipLoosers { get; set; }
    }
}
