using Codificar.Domain.Enums;

namespace Codificar.Application.Chamados;

public sealed record ChamadoListItemDto(
	int Id,
	string Titulo,
	Prioridade Prioridade,
	StatusChamado Status,
	string Solicitante,
	string ResponsavelNome,
	DateTime AbertoEm);
