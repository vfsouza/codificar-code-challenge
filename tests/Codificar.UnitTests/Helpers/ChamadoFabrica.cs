using Codificar.Domain.Entities;
using Codificar.Domain.Enums;

namespace Codificar.UnitTests.Helpers;

public static class ChamadoFabrica {
	public static readonly DateTime Agora = new(2026, 9, 28, 12, 0, 0, DateTimeKind.Utc);

	// Responsáveis criados pelo seed da migration
	public const int Ana = 1;
	public const int Bruno = 2;
	public const int Carla = 3;
	public const int Diego = 4;

	public static Chamado Novo(
		int responsavelId = Ana,
		StatusChamado status = StatusChamado.Aberto,
		Prioridade prioridade = Prioridade.Media,
		string titulo = "Impressora não imprime",
		string solicitante = "Juliana (RH)",
		DateTime? abertoEm = null) {
		var momento = abertoEm ?? Agora;
		var chamado = Chamado.Abrir(titulo, "Descrição do problema", prioridade, solicitante, responsavelId, momento);

		foreach (var etapa in CaminhoAte(status)) {
			momento = momento.AddMinutes(10);
			chamado.AlterarStatus(etapa, momento);
		}

		return chamado;
	}

	private static StatusChamado[] CaminhoAte(StatusChamado destino) => destino switch {
		StatusChamado.Aberto => [],
		StatusChamado.EmAndamento => [StatusChamado.EmAndamento],
		StatusChamado.Resolvido => [StatusChamado.EmAndamento, StatusChamado.Resolvido],
		StatusChamado.Fechado => [StatusChamado.EmAndamento, StatusChamado.Resolvido, StatusChamado.Fechado],
		_ => throw new ArgumentOutOfRangeException(nameof(destino), destino, null)
	};
}
