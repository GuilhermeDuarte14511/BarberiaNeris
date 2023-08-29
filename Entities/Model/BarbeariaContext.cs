using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Model
{
    public class BarbeariaContext : DbContext
    {
        public BarbeariaContext(DbContextOptions<BarbeariaContext> options) : base(options)
        {
        }

        public DbSet<Barbeiro> Barbeiros { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Agendamento> Agendamentos { get; set; }
    }
}
