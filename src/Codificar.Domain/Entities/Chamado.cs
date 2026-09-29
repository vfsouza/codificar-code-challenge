using Codificar.Domain.Enums;
using Codificar.Domain.Exceptions;
using Codificar.Domain.Extensions;

namespace Codificar.Domain.Entities;

public class Chamado {
	private static readonly Dictionary<StatusChamado, StatusChamado[]> TransicoesPermitidas = new() {
		[StatusChamado.Aberto] = [StatusChamado.EmAndamento],
		[StatusChamado.EmAndamento] = [StatusChamado.Resolvido],
		[StatusChamado.Resolvido] = [StatusChamado.Fechado, StatusChamado.EmAndamento],
		[StatusChamado.Fechado] = [],
	};

	public const int TituloTamanhoMaximo = 150;
	public const int DescricaoTamanhoMaximo = 2000;
	public const int SolicitanteTamanhoMaximo = 100;

	public int Id { get; private set; }
	public string Titulo { get; private set; } = null!;
	public string Descricao { get; private set; } = null!;
	public Prioridade Prioridade { get; private set; }
	public StatusChamado Status { get; private set; }
	public string Solicitante { get; private set; } = null!;
	public DateTime AbertoEm { get; private set; }
	public DateTime? AtualizadoEm { get; private set; }
	public DateTime? ConcluidoEm { get; private set; }

	public int ResponsavelId { get; private set; }
	public Responsavel Responsavel { get; private set; } = null!;

	private Chamado() {
	}

	public static Chamado Abrir(string titulo, string descricao, Prioridade prioridade, string solicitante, int responsavelId, DateTime abertoEm) {
		var chamado = new Chamado {
			Status = StatusChamado.Aberto,
			AbertoEm = abertoEm
		};
		chamado.DefinirDados(titulo, descricao, prioridade, solicitante);
		chamado.DefinirResponsavel(responsavelId);
		return chamado;
	}

	public void Editar(string titulo, string descricao, Prioridade prioridade, string solicitante, DateTime momento) {
		DefinirDados(titulo, descricao, prioridade, solicitante);
		AtualizadoEm = momento;
	}

	public void AtribuirResponsavel(int responsavelId, DateTime momento) {
		if (!Status.EstaPendente())
			throw new DomainException("Não é possível reatribuir um chamado já concluído.");

		DefinirResponsavel(responsavelId);
		AtualizadoEm = momento;
	}

	public void AlterarStatus(StatusChamado novoStatus, DateTime momento) {
		if (!TransicoesPermitidas[Status].Contains(novoStatus))
			throw new TransicaoStatusInvalidaException(Status, novoStatus);

		Status = novoStatus;
		AtualizadoEm = momento;

		if (novoStatus == StatusChamado.Resolvido)
			ConcluidoEm = momento;
		else if (novoStatus.EstaPendente())
			ConcluidoEm = null; // reaberto
	}

	public bool PodeTransicionarPara(StatusChamado novoStatus)
		=> TransicoesPermitidas[Status].Contains(novoStatus);

	public IReadOnlyCollection<StatusChamado> ProximosStatusPossiveis => TransicoesPermitidas[Status];

	private void DefinirDados(string titulo, string descricao, Prioridade prioridade, string solicitante) {
		Titulo = ObrigatorioOuErro(titulo, nameof(Titulo));
		Descricao = ObrigatorioOuErro(descricao, nameof(Descricao));
		Solicitante = ObrigatorioOuErro(solicitante, nameof(Solicitante));
		Prioridade = prioridade;
	}

	private void DefinirResponsavel(int responsavelId) {
		if (responsavelId <= 0)
			throw new DomainException("Responsável inválido.");
		ResponsavelId = responsavelId;
	}

	private static string ObrigatorioOuErro(string valor, string campo)
		=> string.IsNullOrWhiteSpace(valor)
			? throw new DomainException($"{campo} é obrigatório.")
			: valor.Trim();
}
