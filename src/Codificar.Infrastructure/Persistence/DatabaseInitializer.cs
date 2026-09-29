using Codificar.Domain.Entities;
using Codificar.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Codificar.Infrastructure.Persistence;

public sealed class DatabaseInitializer(
	IDbContextFactory<ChamadosDbContext> dbFactory,
	TimeProvider timeProvider,
	ILogger<DatabaseInitializer> logger) {
	public async Task InitializeAsync(CancellationToken cancellationToken = default) {
		await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

		logger.LogInformation("Aplicando migrations do banco de dados.");
		await db.Database.MigrateAsync(cancellationToken);

		if (await db.Chamados.AnyAsync(cancellationToken))
			return;

		logger.LogInformation("Banco vazio. Criando chamados de exemplo.");
		db.Chamados.AddRange(CriarChamadosExemplo());
		await db.SaveChangesAsync(cancellationToken);
	}

	private List<Chamado> CriarChamadosExemplo() {
		var agora = timeProvider.GetUtcNow().UtcDateTime;

		// Carga pendente resultante: Ana 3, Carla 2, Bruno 1, Diego 0.
		// Diego tem chamados resolvidos/fechados, mas nenhum pendente:
		// o próximo chamado automático vai para ele, o que demonstra a regra de "em aberto".
		return [
			Criar(
					"Computador travando ao abrir o ERP",
					"Trava sempre que abro o módulo de notas fiscais.",
					Prioridade.Alta,
					"Marcos (Financeiro)",
					responsavelId: 1,
					horasAtras: 3
				),
				Criar(
					"Impressora do 2º andar não imprime",
					"Documentos ficam presos na fila de impressão.",
					Prioridade.Media,
					"Juliana (RH)",
					responsavelId: 1,
					horasAtras: 20,
					StatusChamado.EmAndamento
				),
				Criar(
					"Acesso à pasta compartilhada",
					"Preciso de acesso à pasta do financeiro na rede.",
					Prioridade.Baixa,
					"Pedro (Comercial)",
					responsavelId: 1,
					horasAtras: 30
				),
				Criar(
					"Cadeira quebrada",
					"O encosto da cadeira soltou.",
					Prioridade.Baixa,
					"Fernanda (Administrativo)",
					responsavelId: 2,
					horasAtras: 48
				),
				Criar(
					"Monitor piscando",
					"O monitor principal pisca a cada poucos minutos.",
					Prioridade.Media,
					"Lucas (TI)",
					responsavelId: 3,
					horasAtras: 5,
					StatusChamado.EmAndamento
				),
				Criar(
					"Instalar Teams no notebook novo",
					"Notebook entregue ontem, sem os programas.",
					Prioridade.Media,
					"Renata (Diretoria)",
					responsavelId: 3,
					horasAtras: 8
				),
				Criar(
					"Troca de toner",
					"Toner da impressora da recepção acabou.",
					Prioridade.Baixa,
					"Recepção",
					responsavelId: 2,
					horasAtras: 72,
					StatusChamado.Resolvido
				),
				Criar(
					"Mouse sem fio parou",
					"Já troquei a pilha e não voltou.",
					Prioridade.Baixa,
					"Marcos (Financeiro)",
					responsavelId: 4,
					horasAtras: 96,
					StatusChamado.Fechado
				),
				Criar(
					"Ar-condicionado da sala de reunião",
					"Pingando água em cima da mesa.",
					Prioridade.Alta,
					"Renata (Diretoria)",
					responsavelId: 4,
					horasAtras: 60,
					StatusChamado.Resolvido
				),
			];

		Chamado Criar(string titulo, string descricao, Prioridade prioridade, string solicitante,
			int responsavelId, double horasAtras, StatusChamado status = StatusChamado.Aberto) {
			var abertoEm = agora.AddHours(-horasAtras);
			var chamado = Chamado.Abrir(titulo, descricao, prioridade, solicitante, responsavelId, abertoEm);

			var momento = abertoEm;
			foreach (var etapa in CaminhoAte(status)) {
				momento = momento.AddHours(1);
				chamado.AlterarStatus(etapa, momento);
			}

			return chamado;
		}
	}

	private static StatusChamado[] CaminhoAte(StatusChamado destino) => destino switch {
		StatusChamado.Aberto => [],
		StatusChamado.EmAndamento => [StatusChamado.EmAndamento],
		StatusChamado.Resolvido => [StatusChamado.EmAndamento, StatusChamado.Resolvido],
		StatusChamado.Fechado => [StatusChamado.EmAndamento, StatusChamado.Resolvido, StatusChamado.Fechado],
		_ => throw new ArgumentOutOfRangeException(nameof(destino), destino, null)
	};
}
