using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Model
{
    public class AgendamentoServico
    {
        public int AgendamentoID { get; set; }
        public Agendamento Agendamento { get; set; }

        public int ServicoID { get; set; }
        public Servico Servico { get; set; }
    }

}
