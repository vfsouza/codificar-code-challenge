using Codificar.Application.Abstractions;
using Codificar.Domain.Entities;
using Codificar.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Codificar.UnitTests.Helpers;

// SQLite em memória em vez do provider InMemory: respeita FKs e traduz o LINQ para SQL de verdade.
// O banco vive enquanto a conexão estiver aberta; cada teste cria o seu.
public sealed class BancoEmMemoria : IDisposable {
	private readonly SqliteConnection _conexao = new("DataSource=:memory:");
	private readonly DbContextOptions<ChamadosDbContext> _opcoes;

	public BancoEmMemoria() {
		_conexao.Open();
		_opcoes = new DbContextOptionsBuilder<ChamadosDbContext>().UseSqlite(_conexao).Options;

		using var db = CriarContexto();
		db.Database.EnsureCreated();

		Factory = new Fabrica(this);
	}

	public IChamadosDbContextFactory Factory { get; }

	public ChamadosDbContext CriarContexto() => new(_opcoes);

	public Chamado Adicionar(Chamado chamado) {
		using var db = CriarContexto();
		db.Chamados.Add(chamado);
		db.SaveChanges();
		return chamado;
	}

	public void DesativarResponsavel(int id) {
		using var db = CriarContexto();
		db.Responsaveis.Single(r => r.Id == id).Ativo = false;
		db.SaveChanges();
	}

	public Chamado Buscar(int id) {
		using var db = CriarContexto();
		return db.Chamados.AsNoTracking().Single(c => c.Id == id);
	}

	public void Dispose() => _conexao.Dispose();

	private sealed class Fabrica(BancoEmMemoria banco) : IChamadosDbContextFactory {
		public Task<IChamadosDbContext> CreateAsync(CancellationToken cancellationToken = default)
			=> Task.FromResult<IChamadosDbContext>(banco.CriarContexto());
	}
}
