using Codificar.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Codificar.Application.Responsaveis;

public interface IResponsavelService {
	Task<IReadOnlyList<ResponsavelDto>> ListarAtivosAsync(CancellationToken cancellationToken = default);

	Task<IReadOnlyList<CargaResponsavelDto>> ObterCargaAsync(CancellationToken cancellationToken = default);
}

public sealed class ResponsavelService(IChamadosDbContextFactory dbFactory) : IResponsavelService {
	public async Task<IReadOnlyList<ResponsavelDto>> ListarAtivosAsync(CancellationToken cancellationToken = default) {
		await using var db = await dbFactory.CreateAsync(cancellationToken);

		return await db.Responsaveis
			.Where(r => r.Ativo)
			.OrderBy(r => r.Nome)
			.Select(r => new ResponsavelDto(r.Id, r.Nome))
			.ToListAsync(cancellationToken);
	}

	public async Task<IReadOnlyList<CargaResponsavelDto>> ObterCargaAsync(CancellationToken cancellationToken = default) {
		await using var db = await dbFactory.CreateAsync(cancellationToken);

		return await db.CargaDosAtivos().ToListAsync(cancellationToken);
	}
}
