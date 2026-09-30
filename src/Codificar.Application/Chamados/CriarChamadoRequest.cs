using System.ComponentModel.DataAnnotations;
using Codificar.Domain.Entities;
using Codificar.Domain.Enums;

namespace Codificar.Application.Chamados;

public class CriarChamadoRequest {
	[Required(ErrorMessage = "Informe o título.")]
	[MaxLength(Chamado.TituloTamanhoMaximo, ErrorMessage = "O título deve ter no máximo {1} caracteres.")]
	public string Titulo { get; set; } = string.Empty;

	[Required(ErrorMessage = "Informe a descrição.")]
	[MaxLength(Chamado.DescricaoTamanhoMaximo, ErrorMessage = "A descrição deve ter no máximo {1} caracteres.")]
	public string Descricao { get; set; } = string.Empty;

	public Prioridade Prioridade { get; set; } = Prioridade.Media;

	[Required(ErrorMessage = "Informe o solicitante.")]
	[MaxLength(Chamado.SolicitanteTamanhoMaximo, ErrorMessage = "O solicitante deve ter no máximo {1} caracteres.")]
	public string Solicitante { get; set; } = string.Empty;

	// null = distribuição automática
	public int? ResponsavelId { get; set; }
}
