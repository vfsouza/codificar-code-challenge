using Codificar.Application.Abstractions;
using Codificar.Application.Distribuicao;
using Codificar.Domain.Entities;
using Codificar.Domain.Enums;
using Codificar.Domain.Exceptions;
using Codificar.Domain.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Codificar.Application.Chamados;

public interface IChamadoService {
	Task<IReadOnlyList<ChamadoListItemDto>> ListarAsync(ChamadoFiltro filtro, CancellationToken cancellationToken = default);
	Task<ChamadoDetalheDto?> ObterAsync(int id, CancellationToken cancellationToken = default);
	Task<int> CriarAsync(CriarChamadoRequest request, CancellationToken cancellationToken = default);
	Task EditarAsync(int id, EditarChamadoRequest request, CancellationToken cancellationToken = default);
	Task AlterarStatusAsync(int id, StatusChamado novoStatus, CancellationToken cancellationToken = default);
}

public sealed class ChamadoService(
	IChamadosDbContextFactory dbFactory,
	IDistribuidorChamados distribuidor,
	TimeProvider timeProvider) : IChamadoService {

	public async Task<IReadOnlyList<ChamadoListItemDto>> ListarAsync(ChamadoFiltro filtro, CancellationToken cancellationToken = default) {
		await using var db = await dbFactory.CreateAsync(cancellationToken);

		var query = db.Chamados.AsQueryable();

		if (filtro.Status.Count > 0) {
			var status = filtro.Status;
			query = query.Where(c => status.Contains(c.Status));
		}

		if (filtro.Prioridade is { } prioridade)
			query = query.Where(c => c.Prioridade == prioridade);

		if (filtro.ResponsavelId is { } responsavelId)
			query = query.Where(c => c.ResponsavelId == responsavelId);

		if (!string.IsNullOrWhiteSpace(filtro.Busca)) {
			var padrao = $"%{filtro.Busca.Trim()}%";
			query = query.Where(c => EF.Functions.Like(c.Titulo, padrao) || EF.Functions.Like(c.Solicitante, padrao));
		}

		return await query
			.OrderByDescending(c => StatusChamadoExtensions.Pendentes.Contains(c.Status))
			.ThenByDescending(c => c.Prioridade == Prioridade.Alta ? 3 : c.Prioridade == Prioridade.Media ? 2 : 1)
			.ThenBy(c => c.AbertoEm)
			.Select(c => new ChamadoListItemDto(
				c.Id, c.Titulo, c.Prioridade, c.Status, c.Solicitante, c.Responsavel.Nome, c.AbertoEm))
			.ToListAsync(cancellationToken);
	}

	public async Task<ChamadoDetalheDto?> ObterAsync(int id, CancellationToken cancellationToken = default) {
		await using var db = await dbFactory.CreateAsync(cancellationToken);

		var chamado = await db.Chamados
			.AsNoTracking()
			.Include(c => c.Responsavel)
			.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

		return chamado is null ? null : ParaDetalhe(chamado);
	}

	public async Task<int> CriarAsync(CriarChamadoRequest request, CancellationToken cancellationToken = default) {
		var responsavelId = request.ResponsavelId ?? await distribuidor.EscolherResponsavelAsync(cancellationToken);

		await using var db = await dbFactory.CreateAsync(cancellationToken);

		if (request.ResponsavelId is not null)
			await GarantirResponsavelAtivoAsync(db, responsavelId, cancellationToken);

		var chamado = Chamado.Abrir(
			request.Titulo, request.Descricao, request.Prioridade, request.Solicitante, responsavelId, Agora());

		db.Chamados.Add(chamado);
		await db.SaveChangesAsync(cancellationToken);

		return chamado.Id;
	}

	public async Task EditarAsync(int id, EditarChamadoRequest request, CancellationToken cancellationToken = default) {
		await using var db = await dbFactory.CreateAsync(cancellationToken);

		var chamado = await ObterEntidadeAsync(db, id, cancellationToken);
		var agora = Agora();

		chamado.Editar(request.Titulo, request.Descricao, request.Prioridade, request.Solicitante, agora);

		// Só é possível reatribuir um chamado pendente: ao reabrir, muda o status antes;
		// ao concluir, reatribui antes.
		if (request.Status.EstaPendente()) {
			MudarStatusSeNecessario(chamado, request.Status, agora);
			await ReatribuirSeNecessarioAsync(db, chamado, request.ResponsavelId, agora, cancellationToken);
		} else {
			await ReatribuirSeNecessarioAsync(db, chamado, request.ResponsavelId, agora, cancellationToken);
			MudarStatusSeNecessario(chamado, request.Status, agora);
		}

		await db.SaveChangesAsync(cancellationToken);
	}

	public async Task AlterarStatusAsync(int id, StatusChamado novoStatus, CancellationToken cancellationToken = default) {
		await using var db = await dbFactory.CreateAsync(cancellationToken);

		var chamado = await ObterEntidadeAsync(db, id, cancellationToken);
		chamado.AlterarStatus(novoStatus, Agora());

		await db.SaveChangesAsync(cancellationToken);
	}

	private DateTime Agora() => timeProvider.GetUtcNow().UtcDateTime;

	private static async Task<Chamado> ObterEntidadeAsync(IChamadosDbContext db, int id, CancellationToken cancellationToken)
		=> await db.Chamados.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
			?? throw new DomainException($"Chamado #{id} não encontrado.");

	private static async Task GarantirResponsavelAtivoAsync(IChamadosDbContext db, int responsavelId, CancellationToken cancellationToken) {
		if (!await db.Responsaveis.AnyAsync(r => r.Id == responsavelId && r.Ativo, cancellationToken))
			throw new DomainException("Responsável inexistente ou inativo.");
	}

	private static async Task ReatribuirSeNecessarioAsync(
		IChamadosDbContext db, Chamado chamado, int? responsavelId, DateTime agora, CancellationToken cancellationToken) {
		if (responsavelId is not { } novoId || novoId == chamado.ResponsavelId)
			return;

		await GarantirResponsavelAtivoAsync(db, novoId, cancellationToken);
		chamado.AtribuirResponsavel(novoId, agora);
	}

	private static void MudarStatusSeNecessario(Chamado chamado, StatusChamado novoStatus, DateTime agora) {
		if (novoStatus != chamado.Status)
			chamado.AlterarStatus(novoStatus, agora);
	}

	private static ChamadoDetalheDto ParaDetalhe(Chamado c) => new(
		c.Id, c.Titulo, c.Descricao, c.Prioridade, c.Status, c.Solicitante,
		c.ResponsavelId, c.Responsavel.Nome,
		c.AbertoEm, c.AtualizadoEm, c.ConcluidoEm,
		c.ProximosStatusPossiveis);
}
