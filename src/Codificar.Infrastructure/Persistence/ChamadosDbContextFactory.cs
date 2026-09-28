using Microsoft.EntityFrameworkCore;

namespace Codificar.Infrastructure.Persistence {
	internal sealed class ChamadosDbContextFactory(IDbContextFactory<ChamadosDbContext> inner) : IChamadosDbContextFactory {
		public async Task<IChamadosDbContext> CreateAsync(CancellationToken cancellationToken = default)
			=> await inner.CreateDbContextAsync(cancellationToken);
	}
}