using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Windows.System;


namespace ProgettoAPL.Models
{
    public class Progetto
    {
        public int ID { get; set; }
        public string Descrizione { get; set; }
        public Utente Autore { get; set; }
    }
}
