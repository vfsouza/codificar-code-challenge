using Codificar.Application.Chamados;
using Codificar.Application.Responsaveis;
using Codificar.Domain.Exceptions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Codificar.Web.Components.Pages.Chamados;

public partial class ChamadoNovo : ComponentBase {
	[Inject] private IChamadoService ChamadoService { get; set; } = null!;
	[Inject] private IResponsavelService ResponsavelService { get; set; } = null!;
	[Inject] private NavigationManager Navigation { get; set; } = null!;
	[Inject] private ISnackbar Snackbar { get; set; } = null!;

	private readonly CriarChamadoRequest _model = new();
	private IReadOnlyList<ResponsavelDto> _responsaveis = [];
	private bool _salvando;

	protected override async Task OnInitializedAsync()
		=> _responsaveis = await ResponsavelService.ListarAtivosAsync();

	private async Task SalvarAsync() {
		_salvando = true;
		try {
			var id = await ChamadoService.CriarAsync(_model);
			Snackbar.Add($"Chamado #{id} aberto.", Severity.Success);
			Navigation.NavigateTo($"/chamados/{id}");
		} catch (DomainException ex) {
			Snackbar.Add(ex.Message, Severity.Error);
		} finally {
			_salvando = false;
		}
	}

	private void Voltar() => Navigation.NavigateTo("/chamados");
}
