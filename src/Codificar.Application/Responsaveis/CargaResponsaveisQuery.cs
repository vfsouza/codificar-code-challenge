using Codificar.Application.Abstractions;
using Codificar.Domain.Extensions;

namespace Codificar.Application.Responsaveis;

internal static class CargaResponsaveisQuery {
	/// <summary>
	/// Responsáveis ativos com a quantidade de chamados em aberto,
	/// do menos para o mais carregado (empate: menor Id).
	/// </summary>
	public static IQueryable<CargaResponsavelDto> CargaDosAtivos(this IChamadosDbContext db) =>
		db.Responsaveis
			.Where(r => r.Ativo)
			.Select(r => new {
				r.Id,
				r.Nome,
				Pendentes = db.Chamados.Count(c =>
					c.ResponsavelId == r.Id && StatusChamadoExtensions.Pendentes.Contains(c.Status))
			})
			.OrderBy(x => x.Pendentes)
			.ThenBy(x => x.Id)
			.Select(x => new CargaResponsavelDto(x.Id, x.Nome, x.Pendentes));
}
