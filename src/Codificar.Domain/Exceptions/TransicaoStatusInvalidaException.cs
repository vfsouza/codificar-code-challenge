using Codificar.Domain.Enums;
using Codificar.Domain.Extensions;

namespace Codificar.Domain.Exceptions;

public class TransicaoStatusInvalidaException(StatusChamado statusAtual, StatusChamado novoStatus)
	: DomainException($"Não é possível mudar o status de \"{statusAtual.Descricao()}\" para \"{novoStatus.Descricao()}\".") {
	public StatusChamado StatusAtual { get; } = statusAtual;
	public StatusChamado NovoStatus { get; } = novoStatus;
}
