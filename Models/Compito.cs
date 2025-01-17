using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Windows.System;

namespace ProgettoAPL.Models
{
    public class Compito

    {
        public string Descrizione { get; set; }
        public string Commenti { get; set; }
        public string Modifiche { get; set; }
        public Utente CreatoDa { get; set; }
        public Utente AssegnatoDa { get; set; }
        public CodiceSorgente CodiceSorgente { get; set; }
        public Allegato Allegato { get; set; }
    }
}
