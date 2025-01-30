using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgettoAPL.Models
{
    public class Allegato
    {
        public int Id { get; set; }
        public string Link { get; set; }
        public string Descrizione { get; set; }
        public int TaskID { get; set; }
    }
}
