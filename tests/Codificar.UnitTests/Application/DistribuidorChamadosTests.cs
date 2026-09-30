using Codificar.Application.Distribuicao;
using Codificar.Domain.Enums;
using Codificar.Domain.Exceptions;
using Codificar.UnitTests.Helpers;
using static Codificar.UnitTests.Helpers.ChamadoFabrica;

namespace Codificar.UnitTests.Application;

public class DistribuidorChamadosTests : IDisposable {
	private readonly BancoEmMemoria _banco = new();
	private readonly DistribuidorChamados _distribuidor;

	public DistribuidorChamadosTests() {
		_distribuidor = new DistribuidorChamados(_banco.Factory);
	}

	public void Dispose() => _banco.Dispose();

	[Fact]
	public async Task EscolheResponsavelComMenosChamadosEmAberto() {
		_banco.Adicionar(Novo(Ana));
		_banco.Adicionar(Novo(Ana));
		_banco.Adicionar(Novo(Bruno));
		_banco.Adicionar(Novo(Diego));
		_banco.Adicionar(Novo(Diego, StatusChamado.EmAndamento));

		var escolhido = await _distribuidor.EscolherResponsavelAsync();

		Assert.Equal(Carla, escolhido);
	}

	[Fact]
	public async Task EmpateEntreResponsaveis_EscolheMenorId() {
		var escolhido = await _distribuidor.EscolherResponsavelAsync();

		Assert.Equal(Ana, escolhido);
	}

	[Fact]
	public async Task ChamadosResolvidosEFechados_NaoContamComoCarga() {
		_banco.Adicionar(Novo(Ana));
		_banco.Adicionar(Novo(Bruno));
		_banco.Adicionar(Novo(Carla));
		_banco.Adicionar(Novo(Diego, StatusChamado.Resolvido));
		_banco.Adicionar(Novo(Diego, StatusChamado.Fechado));
		_banco.Adicionar(Novo(Diego, StatusChamado.Fechado));

		var escolhido = await _distribuidor.EscolherResponsavelAsync();

		Assert.Equal(Diego, escolhido);
	}

	[Fact]
	public async Task ResponsavelInativo_NaoRecebeChamado() {
		_banco.DesativarResponsavel(Ana);

		var escolhido = await _distribuidor.EscolherResponsavelAsync();

		Assert.Equal(Bruno, escolhido);
	}

	[Fact]
	public async Task SemResponsavelAtivo_LancaDomainException() {
		foreach (var id in new[] { Ana, Bruno, Carla, Diego })
			_banco.DesativarResponsavel(id);

		await Assert.ThrowsAsync<DomainException>(() => _distribuidor.EscolherResponsavelAsync());
	}
}
