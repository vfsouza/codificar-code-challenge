using Codificar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Codificar.Infrastructure {
	public static class DependencyInjection {
		public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString) {
			services.AddDbContextFactory<ChamadosDbContext>(options =>
				options.UseSqlite(connectionString)
			);

			services.AddSingleton<IChamadosDbContextFactory, ChamadosDbContextFactory>();

			return services;
		}
	}
}