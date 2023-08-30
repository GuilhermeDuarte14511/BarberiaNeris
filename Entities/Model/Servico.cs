using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Model
{
    public class Servico
    {
        public int ServicoID { get; set; }
        public string Nome { get; set; }
        public decimal Preco { get; set; }
        public ICollection<AgendamentoServico> AgendamentoServicos { get; set; }
    }
}
