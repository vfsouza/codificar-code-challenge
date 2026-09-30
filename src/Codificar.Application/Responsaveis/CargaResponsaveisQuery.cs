using Codificar.Application.Abstractions;
using Codificar.Domain.Extensions;

namespace Codificar.Application.Responsaveis;

internal static class CargaResponsaveisQuery {
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
