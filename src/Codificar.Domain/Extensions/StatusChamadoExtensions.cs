using Codificar.Domain.Enums;

namespace Codificar.Domain.Extensions {
	public static class StatusChamadoExtensions {
		public static bool EstaPendente(this StatusChamado status) => status switch {
			StatusChamado.Aberto or StatusChamado.EmAndamento => true,
			_ => false
		};

		public static string Descricao(this StatusChamado status) => status switch {
			StatusChamado.Aberto => "Aberto",
			StatusChamado.EmAndamento => "Em andamento",
			StatusChamado.Resolvido => "Resolvido",
			StatusChamado.Fechado => "Fechado",
			_ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
		};
	}
}