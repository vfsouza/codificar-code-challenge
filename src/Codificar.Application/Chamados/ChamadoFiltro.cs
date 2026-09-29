using Codificar.Domain.Enums;
using Codificar.Domain.Extensions;

namespace Codificar.Application.Chamados;

public sealed class ChamadoFiltro {
	/// <summary>Vazio = todos os status. Padrão: apenas os pendentes.</summary>
	public IReadOnlyCollection<StatusChamado> Status { get; set; } = StatusChamadoExtensions.Pendentes;

	public Prioridade? Prioridade { get; set; }

	public int? ResponsavelId { get; set; }

	/// <summary>Busca por trecho do título ou do solicitante.</summary>
	public string? Busca { get; set; }
}
