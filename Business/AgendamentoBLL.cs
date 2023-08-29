using Entities.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    public class AgendamentoBLL
    {
        private readonly BarbeariaContext _context;

        public AgendamentoBLL(BarbeariaContext context)
        {
            _context = context;
        }

        public void Agendar(string name, string email, string phone, string service, int barber, DateTime appointment)
        {
            // Primeiro, verifique se o cliente já existe com base no e-mail
            var cliente = _context.Clientes.FirstOrDefault(c => c.Email == email);

            // Se o cliente não existir, crie um novo
            if (cliente == null)
            {
                cliente = new Cliente
                {
                    Nome = name,
                    Email = email,
                    Telefone = phone
                };
                _context.Clientes.Add(cliente);
                _context.SaveChanges(); // Salve o cliente no banco de dados antes de criar o agendamento
            }

            // Encontre o barbeiro pelo nome
            var barbeiro = _context.Barbeiros.FirstOrDefault(b => b.BarbeiroID == barber);

            // Crie um novo agendamento
            var agendamento = new Agendamento
            {
                ClienteID = cliente.ClienteID,
                BarbeiroID = barbeiro.BarbeiroID,
                DataHora = appointment,
                Servico = service
            };

            _context.Agendamentos.Add(agendamento);

            // Salve as alterações no banco de dados
            _context.SaveChanges();
        }

    }
}
