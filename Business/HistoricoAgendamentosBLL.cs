using Entities.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    public class HistoricoAgendamentosBLL
    {
        private readonly BarbeariaContext _context;

        public HistoricoAgendamentosBLL(BarbeariaContext context)
        {
            _context = context;

        }

        public List<AgendamentoInfo> ObterInformacoesAgendamentosPorCliente(int clienteId)
        {
            var agendamentosInfo = _context.Agendamentos
                .Where(a => a.ClienteID == clienteId)
                .Select(a => new AgendamentoInfo
                {
                    AgendamentoID = a.AgendamentoID,
                    DataHora = a.DataHora,
                    Servico = a.Servico,
                    ValorTotal = a.ValorTotal,
                    NomeBarbeiro = a.Barbeiro.Nome,
                    NomeCliente = a.Cliente.Nome
                })
                .ToList();

            return agendamentosInfo;
        }

    }
}
