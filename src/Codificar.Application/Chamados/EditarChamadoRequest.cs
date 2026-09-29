using Codificar.Domain.Enums;

namespace Codificar.Application.Chamados;

public sealed class EditarChamadoRequest : CriarChamadoRequest {
	public StatusChamado Status { get; set; }

	public static EditarChamadoRequest De(ChamadoDetalheDto chamado) => new() {
		Titulo = chamado.Titulo,
		Descricao = chamado.Descricao,
		Prioridade = chamado.Prioridade,
		Solicitante = chamado.Solicitante,
		ResponsavelId = chamado.ResponsavelId,
		Status = chamado.Status
	};
}
