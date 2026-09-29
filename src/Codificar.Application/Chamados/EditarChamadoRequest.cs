using Codificar.Domain.Enums;

namespace Codificar.Application.Chamados;

public sealed class EditarChamadoRequest : CriarChamadoRequest {
	public StatusChamado Status { get; set; }
}
