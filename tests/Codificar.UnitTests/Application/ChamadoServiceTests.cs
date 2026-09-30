using Codificar.Application.Chamados;
using Codificar.Application.Distribuicao;
using Codificar.Domain.Enums;
using Codificar.Domain.Exceptions;
using Codificar.UnitTests.Helpers;
using static Codificar.UnitTests.Helpers.ChamadoFabrica;

namespace Codificar.UnitTests.Application;

public class ChamadoServiceTests : IDisposable {
	private static readonly DateTime AgoraDoServico = Agora.AddDays(1);

	private readonly BancoEmMemoria _banco = new();
	private readonly ChamadoService _service;

	public ChamadoServiceTests() {
		_service = new ChamadoService(
			_banco.Factory,
			new DistribuidorChamados(_banco.Factory),
			new RelogioFixo(AgoraDoServico));
	}

	public void Dispose() => _banco.Dispose();

	private static CriarChamadoRequest NovoRequest(int? responsavelId = null) => new() {
		Titulo = "Notebook sem internet",
		Descricao = "O wi-fi não conecta",
		Prioridade = Prioridade.Alta,
		Solicitante = "Paula (Compras)",
		ResponsavelId = responsavelId
	};

	private static EditarChamadoRequest EdicaoDe(ChamadoDetalheDto chamado, StatusChamado status, int? responsavelId)
		=> new() {
			Titulo = chamado.Titulo,
			Descricao = chamado.Descricao,
			Prioridade = chamado.Prioridade,
			Solicitante = chamado.Solicitante,
			Status = status,
			ResponsavelId = responsavelId
		};

	[Fact]
	public async Task Criar_SemResponsavel_AtribuiAoDeMenorCarga() {
		_banco.Adicionar(Novo(Ana));
		_banco.Adicionar(Novo(Bruno));
		_banco.Adicionar(Novo(Carla));

		var id = await _service.CriarAsync(NovoRequest());

		Assert.Equal(Diego, _banco.Buscar(id).ResponsavelId);
	}

	[Fact]
	public async Task Criar_ComResponsavelEscolhido_UsaOEscolhido() {
		_banco.Adicionar(Novo(Carla));

		var id = await _service.CriarAsync(NovoRequest(Carla));

		Assert.Equal(Carla, _banco.Buscar(id).ResponsavelId);
	}

	[Fact]
	public async Task Criar_UsaHorarioDoTimeProviderEAbreComoAberto() {
		var id = await _service.CriarAsync(NovoRequest());

		var chamado = _banco.Buscar(id);
		Assert.Equal(AgoraDoServico, chamado.AbertoEm);
		Assert.Equal(DateTimeKind.Utc, chamado.AbertoEm.Kind);
		Assert.Equal(StatusChamado.Aberto, chamado.Status);
	}

	[Theory]
	[InlineData(99)]
	[InlineData(Bruno)]
	public async Task Criar_ComResponsavelInexistenteOuInativo_LancaDomainException(int responsavelId) {
		_banco.DesativarResponsavel(Bruno);

		await Assert.ThrowsAsync<DomainException>(() => _service.CriarAsync(NovoRequest(responsavelId)));
	}

	[Fact]
	public async Task Listar_ComFiltroPadrao_RetornaApenasPendentes() {
		var aberto = _banco.Adicionar(Novo(status: StatusChamado.Aberto));
		var emAndamento = _banco.Adicionar(Novo(status: StatusChamado.EmAndamento));
		_banco.Adicionar(Novo(status: StatusChamado.Resolvido));
		_banco.Adicionar(Novo(status: StatusChamado.Fechado));

		var chamados = await _service.ListarAsync(new ChamadoFiltro());

		Assert.Equal([aberto.Id, emAndamento.Id], chamados.Select(c => c.Id).Order());
	}

	[Fact]
	public async Task Listar_OrdenaPorPendenciaDepoisPrioridadeDepoisMaisAntigo() {
		var baixa = _banco.Adicionar(Novo(prioridade: Prioridade.Baixa, abertoEm: Agora.AddHours(-10)));
		var altaRecente = _banco.Adicionar(Novo(prioridade: Prioridade.Alta, abertoEm: Agora.AddHours(-1)));
		var media = _banco.Adicionar(Novo(prioridade: Prioridade.Media, abertoEm: Agora.AddHours(-2)));
		var altaAntiga = _banco.Adicionar(Novo(prioridade: Prioridade.Alta, abertoEm: Agora.AddHours(-5)));
		var resolvidoAlta = _banco.Adicionar(Novo(status: StatusChamado.Resolvido, prioridade: Prioridade.Alta));

		var chamados = await _service.ListarAsync(new ChamadoFiltro { Status = [] });

		Assert.Equal(
			[altaAntiga.Id, altaRecente.Id, media.Id, baixa.Id, resolvidoAlta.Id],
			chamados.Select(c => c.Id));
	}

	[Fact]
	public async Task Listar_BuscaPorTituloOuSolicitanteSemDiferenciarMaiusculas() {
		var porTitulo = _banco.Adicionar(Novo(titulo: "Impressora travada", solicitante: "Marcos"));
		var porSolicitante = _banco.Adicionar(Novo(titulo: "Mouse quebrado", solicitante: "Setor de impressão"));
		_banco.Adicionar(Novo(titulo: "Cadeira quebrada", solicitante: "Fernanda"));

		var chamados = await _service.ListarAsync(new ChamadoFiltro { Busca = "IMPRESS" });

		Assert.Equal([porTitulo.Id, porSolicitante.Id], chamados.Select(c => c.Id).Order());
	}

	[Fact]
	public async Task Listar_FiltraPorPrioridadeEResponsavel() {
		var esperado = _banco.Adicionar(Novo(Carla, prioridade: Prioridade.Alta));
		_banco.Adicionar(Novo(Carla, prioridade: Prioridade.Baixa));
		_banco.Adicionar(Novo(Ana, prioridade: Prioridade.Alta));

		var chamados = await _service.ListarAsync(new ChamadoFiltro { Prioridade = Prioridade.Alta, ResponsavelId = Carla });

		var chamado = Assert.Single(chamados);
		Assert.Equal(esperado.Id, chamado.Id);
		Assert.Equal("Carla Mendes", chamado.ResponsavelNome);
	}

	[Fact]
	public async Task Obter_TrazResponsavelEProximosStatus() {
		var criado = _banco.Adicionar(Novo(Bruno, StatusChamado.Resolvido));

		var chamado = await _service.ObterAsync(criado.Id);

		Assert.NotNull(chamado);
		Assert.Equal("Bruno Lima", chamado.ResponsavelNome);
		Assert.Equal([StatusChamado.Fechado, StatusChamado.EmAndamento], chamado.ProximosStatusPossiveis);
	}

	[Fact]
	public async Task Obter_ChamadoInexistente_RetornaNull() {
		Assert.Null(await _service.ObterAsync(999));
	}

	[Fact]
	public async Task Editar_ReabrindoETrocandoResponsavel_AplicaOsDois() {
		var criado = _banco.Adicionar(Novo(Ana, StatusChamado.Resolvido));
		var detalhe = (await _service.ObterAsync(criado.Id))!;

		await _service.EditarAsync(criado.Id, EdicaoDe(detalhe, StatusChamado.EmAndamento, Carla));

		var chamado = _banco.Buscar(criado.Id);
		Assert.Equal(StatusChamado.EmAndamento, chamado.Status);
		Assert.Equal(Carla, chamado.ResponsavelId);
		Assert.Null(chamado.ConcluidoEm);
	}

	[Fact]
	public async Task Editar_ResolvendoETrocandoResponsavel_AplicaOsDois() {
		var criado = _banco.Adicionar(Novo(Ana, StatusChamado.EmAndamento));
		var detalhe = (await _service.ObterAsync(criado.Id))!;

		await _service.EditarAsync(criado.Id, EdicaoDe(detalhe, StatusChamado.Resolvido, Diego));

		var chamado = _banco.Buscar(criado.Id);
		Assert.Equal(StatusChamado.Resolvido, chamado.Status);
		Assert.Equal(Diego, chamado.ResponsavelId);
		Assert.Equal(AgoraDoServico, chamado.ConcluidoEm);
	}

	[Fact]
	public async Task Editar_SemResponsavel_MantemOAtual() {
		var criado = _banco.Adicionar(Novo(Bruno));
		var detalhe = (await _service.ObterAsync(criado.Id))!;

		await _service.EditarAsync(criado.Id, EdicaoDe(detalhe, StatusChamado.Aberto, responsavelId: null));

		Assert.Equal(Bruno, _banco.Buscar(criado.Id).ResponsavelId);
	}

	[Fact]
	public async Task Editar_ChamadoInexistente_LancaDomainException() {
		var request = new EditarChamadoRequest { Titulo = "t", Descricao = "d", Solicitante = "s" };

		await Assert.ThrowsAsync<DomainException>(() => _service.EditarAsync(999, request));
	}

	[Fact]
	public async Task AlterarStatus_TransicaoPermitida_Persiste() {
		var criado = _banco.Adicionar(Novo());

		await _service.AlterarStatusAsync(criado.Id, StatusChamado.EmAndamento);

		var chamado = _banco.Buscar(criado.Id);
		Assert.Equal(StatusChamado.EmAndamento, chamado.Status);
		Assert.Equal(AgoraDoServico, chamado.AtualizadoEm);
	}

	[Fact]
	public async Task AlterarStatus_TransicaoInvalida_LancaExcecaoENaoPersiste() {
		var criado = _banco.Adicionar(Novo());

		await Assert.ThrowsAsync<TransicaoStatusInvalidaException>(
			() => _service.AlterarStatusAsync(criado.Id, StatusChamado.Fechado));

		Assert.Equal(StatusChamado.Aberto, _banco.Buscar(criado.Id).Status);
	}
}
