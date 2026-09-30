using Codificar.Application.Responsaveis;
using Codificar.Domain.Enums;
using Codificar.UnitTests.Helpers;
using static Codificar.UnitTests.Helpers.ChamadoFabrica;

namespace Codificar.UnitTests.Application;

public class ResponsavelServiceTests : IDisposable {
	private readonly BancoEmMemoria _banco = new();
	private readonly ResponsavelService _service;

	public ResponsavelServiceTests() {
		_service = new ResponsavelService(_banco.Factory);
	}

	public void Dispose() => _banco.Dispose();

	[Fact]
	public async Task ListarAtivos_RetornaEmOrdemAlfabeticaSemInativos() {
		_banco.DesativarResponsavel(Bruno);

		var responsaveis = await _service.ListarAtivosAsync();

		Assert.Equal(["Ana Souza", "Carla Mendes", "Diego Rocha"], responsaveis.Select(r => r.Nome));
	}

	[Fact]
	public async Task ObterCarga_ContaPendentesEOrdenaComoADistribuicao() {
		_banco.Adicionar(Novo(Ana));
		_banco.Adicionar(Novo(Ana, StatusChamado.EmAndamento));
		_banco.Adicionar(Novo(Bruno));
		_banco.Adicionar(Novo(Carla, StatusChamado.Resolvido));

		var carga = await _service.ObterCargaAsync();

		Assert.Equal(
			[
				new CargaResponsavelDto(Carla, "Carla Mendes", 0),
				new CargaResponsavelDto(Diego, "Diego Rocha", 0),
				new CargaResponsavelDto(Bruno, "Bruno Lima", 1),
				new CargaResponsavelDto(Ana, "Ana Souza", 2)
			],
			carga);
	}
}
