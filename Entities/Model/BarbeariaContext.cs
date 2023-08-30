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
        public DbSet<Servico> Servicos { get; set; }
        public DbSet<AgendamentoServico> AgendamentoServicos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AgendamentoServico>(entity =>
            {
                entity.ToTable("agendamento_servicos");

                // Chave primária composta
                entity.HasKey(e => new { e.AgendamentoID, e.ServicoID })
                    .HasName("PK_agendamento_servicos");

                // Configuração das propriedades
                entity.Property(e => e.AgendamentoID)
                    .HasColumnName("AgendamentoID");

                entity.Property(e => e.ServicoID)
                    .HasColumnName("ServicoID");

                // Relacionamentos
                entity.HasOne(d => d.Agendamento)
                    .WithMany(p => p.AgendamentoServicos)
                    .HasForeignKey(d => d.AgendamentoID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("agendamento_servicos_ibfk_1");

                entity.HasOne(d => d.Servico)
                    .WithMany(p => p.AgendamentoServicos)
                    .HasForeignKey(d => d.ServicoID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("agendamento_servicos_ibfk_2");
            });
        }

    }
}
