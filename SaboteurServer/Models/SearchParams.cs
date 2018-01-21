using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SaboteurServer.Models
{
    public class SearchParams
    {
        [Required]
        public int PartySize { get; set; }
        [Required]
        public bool WithoutDeadlocks { get; set; }
        [Required]
        public bool SkipLoosers { get; set; }
    }
}
