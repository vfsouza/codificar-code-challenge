using Codificar.Application.Chamados;
using Codificar.Application.Responsaveis;
using Codificar.Domain.Enums;
using Microsoft.AspNetCore.Components;

namespace Codificar.Web.Components.Shared;

public partial class FiltrosChamados : ComponentBase {
	[Parameter, EditorRequired]
	public ChamadoFiltro Filtro { get; set; } = new();

	[Parameter]
	public EventCallback<ChamadoFiltro> FiltroChanged { get; set; }

	[Parameter, EditorRequired]
	public IReadOnlyList<ResponsavelDto> Responsaveis { get; set; } = [];

	private Task AlterarBuscaAsync(string? busca) => NotificarAsync(Filtro with { Busca = busca });

	private Task AlterarStatusAsync(IEnumerable<StatusChamado>? status)
		=> NotificarAsync(Filtro with { Status = status?.ToList() ?? [] });

	private Task AlterarPrioridadeAsync(Prioridade? prioridade) => NotificarAsync(Filtro with { Prioridade = prioridade });

	private Task AlterarResponsavelAsync(int? responsavelId) => NotificarAsync(Filtro with { ResponsavelId = responsavelId });

	private Task RestaurarAsync() => NotificarAsync(new ChamadoFiltro());

	private Task NotificarAsync(ChamadoFiltro filtro) => FiltroChanged.InvokeAsync(filtro);

	private string NomeDoResponsavel(int? id) => Responsaveis.FirstOrDefault(r => r.Id == id)?.Nome ?? string.Empty;
}
