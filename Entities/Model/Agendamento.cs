﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Model
{
    public class Agendamento
    {
        public int AgendamentoID { get; set; }
        public DateTime DataHora { get; set; }
        public string Servico { get; set; }

        public int ClienteID { get; set; }
        public Cliente Cliente { get; set; }

        public int BarbeiroID { get; set; }
        public Barbeiro Barbeiro { get; set; }
    }

}
