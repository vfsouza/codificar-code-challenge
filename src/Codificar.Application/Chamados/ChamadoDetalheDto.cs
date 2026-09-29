using Codificar.Domain.Enums;

namespace Codificar.Application.Chamados;

public sealed record ChamadoDetalheDto(
	int Id,
	string Titulo,
	string Descricao,
	Prioridade Prioridade,
	StatusChamado Status,
	string Solicitante,
	int ResponsavelId,
	string ResponsavelNome,
	DateTime AbertoEm,
	DateTime? AtualizadoEm,
	DateTime? ConcluidoEm,
	IReadOnlyCollection<StatusChamado> ProximosStatusPossiveis);
