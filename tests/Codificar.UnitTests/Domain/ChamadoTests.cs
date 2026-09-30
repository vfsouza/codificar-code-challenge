using Codificar.Domain.Entities;
using Codificar.Domain.Enums;
using Codificar.Domain.Exceptions;
using Codificar.UnitTests.Helpers;

namespace Codificar.UnitTests.Domain;

public class ChamadoTests {
	private static readonly DateTime Agora = ChamadoFabrica.Agora;

	[Fact]
	public void Abrir_CriaChamadoAbertoComDadosSemEspacosExtras() {
		var chamado = Chamado.Abrir("  Monitor piscando ", " Pisca a cada minuto ", Prioridade.Alta, " Lucas ", 3, Agora);

		Assert.Equal(StatusChamado.Aberto, chamado.Status);
		Assert.Equal("Monitor piscando", chamado.Titulo);
		Assert.Equal("Pisca a cada minuto", chamado.Descricao);
		Assert.Equal("Lucas", chamado.Solicitante);
		Assert.Equal(Prioridade.Alta, chamado.Prioridade);
		Assert.Equal(3, chamado.ResponsavelId);
		Assert.Equal(Agora, chamado.AbertoEm);
		Assert.Null(chamado.AtualizadoEm);
		Assert.Null(chamado.ConcluidoEm);
	}

	[Theory]
	[InlineData("", "Descrição", "Solicitante")]
	[InlineData("   ", "Descrição", "Solicitante")]
	[InlineData("Título", "", "Solicitante")]
	[InlineData("Título", "Descrição", " ")]
	public void Abrir_SemCampoObrigatorio_LancaDomainException(string titulo, string descricao, string solicitante) {
		Assert.Throws<DomainException>(() => Chamado.Abrir(titulo, descricao, Prioridade.Baixa, solicitante, 1, Agora));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public void Abrir_ComResponsavelInvalido_LancaDomainException(int responsavelId) {
		Assert.Throws<DomainException>(() => Chamado.Abrir("Título", "Descrição", Prioridade.Baixa, "Solicitante", responsavelId, Agora));
	}

	[Theory]
	[InlineData(StatusChamado.Aberto, StatusChamado.EmAndamento)]
	[InlineData(StatusChamado.EmAndamento, StatusChamado.Resolvido)]
	[InlineData(StatusChamado.Resolvido, StatusChamado.Fechado)]
	[InlineData(StatusChamado.Resolvido, StatusChamado.EmAndamento)]
	public void AlterarStatus_TransicaoPermitida_MudaStatus(StatusChamado atual, StatusChamado novo) {
		var chamado = ChamadoFabrica.Novo(status: atual);
		var momento = Agora.AddHours(5);

		chamado.AlterarStatus(novo, momento);

		Assert.Equal(novo, chamado.Status);
		Assert.Equal(momento, chamado.AtualizadoEm);
	}

	[Theory]
	[InlineData(StatusChamado.Aberto, StatusChamado.Resolvido)]
	[InlineData(StatusChamado.Aberto, StatusChamado.Fechado)]
	[InlineData(StatusChamado.EmAndamento, StatusChamado.Aberto)]
	[InlineData(StatusChamado.EmAndamento, StatusChamado.Fechado)]
	[InlineData(StatusChamado.Resolvido, StatusChamado.Aberto)]
	[InlineData(StatusChamado.Fechado, StatusChamado.EmAndamento)]
	[InlineData(StatusChamado.Fechado, StatusChamado.Aberto)]
	public void AlterarStatus_TransicaoNaoPermitida_LancaExcecaoEMantemStatus(StatusChamado atual, StatusChamado novo) {
		var chamado = ChamadoFabrica.Novo(status: atual);

		var excecao = Assert.Throws<TransicaoStatusInvalidaException>(() => chamado.AlterarStatus(novo, Agora));

		Assert.Equal(atual, chamado.Status);
		Assert.Equal(atual, excecao.StatusAtual);
		Assert.Equal(novo, excecao.NovoStatus);
	}

	[Fact]
	public void AlterarStatus_ParaResolvido_PreencheConcluidoEm() {
		var chamado = ChamadoFabrica.Novo(status: StatusChamado.EmAndamento);
		var momento = Agora.AddHours(2);

		chamado.AlterarStatus(StatusChamado.Resolvido, momento);

		Assert.Equal(momento, chamado.ConcluidoEm);
	}

	[Fact]
	public void AlterarStatus_AoReabrir_LimpaConcluidoEm() {
		var chamado = ChamadoFabrica.Novo(status: StatusChamado.Resolvido);

		chamado.AlterarStatus(StatusChamado.EmAndamento, Agora.AddHours(3));

		Assert.Null(chamado.ConcluidoEm);
	}

	[Fact]
	public void AlterarStatus_AoFechar_MantemDataDeConclusao() {
		var chamado = ChamadoFabrica.Novo(status: StatusChamado.Resolvido);
		var concluidoEm = chamado.ConcluidoEm;

		chamado.AlterarStatus(StatusChamado.Fechado, Agora.AddDays(1));

		Assert.Equal(concluidoEm, chamado.ConcluidoEm);
	}

	[Theory]
	[InlineData(StatusChamado.Aberto)]
	[InlineData(StatusChamado.EmAndamento)]
	public void AtribuirResponsavel_ChamadoPendente_TrocaResponsavel(StatusChamado status) {
		var chamado = ChamadoFabrica.Novo(responsavelId: 1, status: status);

		chamado.AtribuirResponsavel(2, Agora.AddHours(1));

		Assert.Equal(2, chamado.ResponsavelId);
	}

	[Theory]
	[InlineData(StatusChamado.Resolvido)]
	[InlineData(StatusChamado.Fechado)]
	public void AtribuirResponsavel_ChamadoConcluido_LancaDomainException(StatusChamado status) {
		var chamado = ChamadoFabrica.Novo(responsavelId: 1, status: status);

		Assert.Throws<DomainException>(() => chamado.AtribuirResponsavel(2, Agora));
		Assert.Equal(1, chamado.ResponsavelId);
	}

	[Fact]
	public void Editar_AtualizaDadosEDataDeAtualizacao() {
		var chamado = ChamadoFabrica.Novo();
		var momento = Agora.AddHours(1);

		chamado.Editar("Novo título", "Nova descrição", Prioridade.Alta, "Pedro", momento);

		Assert.Equal("Novo título", chamado.Titulo);
		Assert.Equal("Nova descrição", chamado.Descricao);
		Assert.Equal(Prioridade.Alta, chamado.Prioridade);
		Assert.Equal("Pedro", chamado.Solicitante);
		Assert.Equal(momento, chamado.AtualizadoEm);
	}

	[Theory]
	[InlineData(StatusChamado.Aberto, new[] { StatusChamado.EmAndamento })]
	[InlineData(StatusChamado.EmAndamento, new[] { StatusChamado.Resolvido })]
	[InlineData(StatusChamado.Resolvido, new[] { StatusChamado.Fechado, StatusChamado.EmAndamento })]
	[InlineData(StatusChamado.Fechado, new StatusChamado[0])]
	public void ProximosStatusPossiveis_SegueFluxoDeStatus(StatusChamado status, StatusChamado[] esperados) {
		var chamado = ChamadoFabrica.Novo(status: status);

		Assert.Equal(esperados, chamado.ProximosStatusPossiveis);
	}
}
