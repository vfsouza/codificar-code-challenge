using Codificar.Domain.Enums;

namespace Codificar.Domain.Extensions;

public static class StatusChamadoExtensions {
	/// <summary>
	/// Status considerados "em aberto": o trabalho do responsável ainda não terminou.
	/// Fonte única da regra, usada pela entidade, pelo distribuidor e pelos filtros.
	/// </summary>
	public static readonly IReadOnlyCollection<StatusChamado> Pendentes = [StatusChamado.Aberto, StatusChamado.EmAndamento];

	public static bool EstaPendente(this StatusChamado status) => Pendentes.Contains(status);

	public static string Descricao(this StatusChamado status) => status switch {
		StatusChamado.Aberto => "Aberto",
		StatusChamado.EmAndamento => "Em andamento",
		StatusChamado.Resolvido => "Resolvido",
		StatusChamado.Fechado => "Fechado",
		_ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
	};
}

