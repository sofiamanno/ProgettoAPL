﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgettoAPL.Models
{
    public class Utente
    {
        public int Id { get; set; } 
        public string Username { get; set; }
        public string Pwd { get; set; }
        public string Email { get; set; }
    }
}
