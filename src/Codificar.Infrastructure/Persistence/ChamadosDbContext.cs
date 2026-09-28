using Codificar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Codificar.Infrastructure.Persistence {
	public sealed class ChamadosDbContext(DbContextOptions<ChamadosDbContext> options) : DbContext(options), IChamadosDbContext {

		public DbSet<Chamado> Chamados => Set<Chamado>();
		public DbSet<Responsavel> Responsaveis => Set<Responsavel>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
			=> modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChamadosDbContext).Assembly);
	}
}