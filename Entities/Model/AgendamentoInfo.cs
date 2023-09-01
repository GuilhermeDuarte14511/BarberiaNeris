using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Model
{
    public class AgendamentoInfo
    {
        public int AgendamentoID { get; set; }
        public DateTime DataHora { get; set; }
        public string Servico { get; set; }
        public decimal ValorTotal { get; set; }
        public string NomeBarbeiro { get; set; }
        public string NomeCliente { get; set; }
    }

}
