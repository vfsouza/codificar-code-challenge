using Codificar.Domain.Enums;

namespace Codificar.Domain.Exceptions {
	public class TransicaoStatusInvalidaException(StatusChamado status, StatusChamado novoStatus) : Exception(message: $"{status}: {novoStatus}") {

	}
}