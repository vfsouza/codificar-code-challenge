using Codificar.Application.Chamados;
using Codificar.Application.Responsaveis;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Codificar.Web.Components.Pages.Chamados;

public partial class ChamadosLista : ComponentBase {
	[Inject] private IChamadoService ChamadoService { get; set; } = null!;
	[Inject] private IResponsavelService ResponsavelService { get; set; } = null!;
	[Inject] private NavigationManager Navigation { get; set; } = null!;

	private ChamadoFiltro _filtro = new();
	private IReadOnlyList<ChamadoListItemDto> _chamados = [];
	private IReadOnlyList<CargaResponsavelDto> _cargas = [];
	private IReadOnlyList<ResponsavelDto> _responsaveis = [];
	private bool _carregando = true;

	protected override async Task OnInitializedAsync() {
		_responsaveis = await ResponsavelService.ListarAtivosAsync();
		_cargas = await ResponsavelService.ObterCargaAsync();
		await CarregarChamadosAsync();
	}

	private async Task CarregarChamadosAsync() {
		_carregando = true;
		_chamados = await ChamadoService.ListarAsync(_filtro);
		_carregando = false;
	}

	private void AbrirDetalhe(TableRowClickEventArgs<ChamadoListItemDto> args)
		=> Navigation.NavigateTo($"/chamados/{args.Item!.Id}");
}
