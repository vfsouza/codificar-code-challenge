using Codificar.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Codificar.Application.Responsaveis;

public interface IResponsavelService {
	/// <summary>Responsáveis que podem receber chamados, em ordem alfabética.</summary>
	Task<IReadOnlyList<ResponsavelDto>> ListarAtivosAsync(CancellationToken cancellationToken = default);

	/// <summary>Chamados em aberto por responsável; o primeiro é o próximo da distribuição automática.</summary>
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
