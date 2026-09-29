using Codificar.Domain.Enums;

namespace Codificar.Domain.Extensions;

public static class PrioridadeExtensions {
	public static string Descricao(this Prioridade prioridade) => prioridade switch {
		Prioridade.Baixa => "Baixa",
		Prioridade.Media => "Média",
		Prioridade.Alta => "Alta",
		_ => throw new ArgumentOutOfRangeException(nameof(prioridade), prioridade, null)
	};
}
