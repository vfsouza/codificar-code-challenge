using Codificar.Application.Chamados;
using Codificar.Application.Distribuicao;
using Codificar.Application.Responsaveis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Codificar.Application;

public static class DependencyInjection {
	public static IServiceCollection AddApplication(this IServiceCollection services) {
		services.TryAddSingleton(TimeProvider.System);

		services.AddScoped<IChamadoService, ChamadoService>();
		services.AddScoped<IResponsavelService, ResponsavelService>();
		services.AddScoped<IDistribuidorChamados, DistribuidorChamados>();

		return services;
	}
}
