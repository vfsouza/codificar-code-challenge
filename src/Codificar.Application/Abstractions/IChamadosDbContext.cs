using Codificar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Codificar.Infrastructure.Persistence;

public interface IChamadosDbContext : IAsyncDisposable {
	DbSet<Chamado> Chamados { get; }
	DbSet<Responsavel> Responsaveis { get; }
	Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IChamadosDbContextFactory {
	Task<IChamadosDbContext> CreateAsync(CancellationToken cancellationToken = default);
}
