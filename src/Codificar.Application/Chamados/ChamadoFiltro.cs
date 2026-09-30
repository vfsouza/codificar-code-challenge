using Codificar.Domain.Enums;
using Codificar.Domain.Extensions;

namespace Codificar.Application.Chamados;

public sealed record ChamadoFiltro {
	public IReadOnlyCollection<StatusChamado> Status { get; set; } = StatusChamadoExtensions.Pendentes;

	public Prioridade? Prioridade { get; set; }

	public int? ResponsavelId { get; set; }

	public string? Busca { get; set; }
}
