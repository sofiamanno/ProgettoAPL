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
        public int ID { get; set; }
        public string Descrizione { get; set; }
        public string Commenti { get; set; }
      
        public bool Completato { get; set; }
        public int AutoreID { get; set; }
        public int IncaricatoID { get; set; }
        public int CodeID { get; set; }
        public int ProgettoID { get; set; }
    }
}
