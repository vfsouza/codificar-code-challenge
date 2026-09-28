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
			services.AddTransient<DatabaseInitializer>();

			return services;
		}

		public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default) {
			await using var scope = services.CreateAsyncScope();
			var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
			await initializer.InitializeAsync(cancellationToken);
		}
	}
}