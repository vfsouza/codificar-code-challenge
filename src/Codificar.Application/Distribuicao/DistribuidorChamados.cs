using Codificar.Application.Abstractions;
using Codificar.Application.Responsaveis;
using Codificar.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Codificar.Application.Distribuicao;

public interface IDistribuidorChamados {
	/// <summary>
	/// Retorna o Id do responsável ativo com menos chamados em aberto.
	/// Empate: menor Id.
	/// </summary>
	/// <exception cref="DomainException">Quando não há nenhum responsável ativo.</exception>
	Task<int> EscolherResponsavelAsync(CancellationToken cancellationToken = default);
}

public sealed class DistribuidorChamados(IChamadosDbContextFactory dbFactory) : IDistribuidorChamados {
	public async Task<int> EscolherResponsavelAsync(CancellationToken cancellationToken = default) {
		await using var db = await dbFactory.CreateAsync(cancellationToken);

		var responsavelId = await db.CargaDosAtivos()
			.Select(c => (int?)c.ResponsavelId)
			.FirstOrDefaultAsync(cancellationToken);

		return responsavelId ?? throw new DomainException("Nenhum responsável ativo disponível para atribuição automática.");
	}
}
