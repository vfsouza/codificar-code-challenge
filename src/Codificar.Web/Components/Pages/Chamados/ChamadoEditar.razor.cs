using Codificar.Application.Chamados;
using Codificar.Application.Responsaveis;
using Codificar.Domain.Enums;
using Codificar.Domain.Exceptions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Codificar.Web.Components.Pages.Chamados;

public partial class ChamadoEditar : ComponentBase {
	[Inject] private IChamadoService ChamadoService { get; set; } = null!;
	[Inject] private IResponsavelService ResponsavelService { get; set; } = null!;
	[Inject] private NavigationManager Navigation { get; set; } = null!;
	[Inject] private ISnackbar Snackbar { get; set; } = null!;

	[Parameter]
	public int Id { get; set; }

	private EditarChamadoRequest? _model;
	private IReadOnlyList<ResponsavelDto> _responsaveis = [];
	private IReadOnlyCollection<StatusChamado> _statusDisponiveis = [];
	private bool _carregando = true;
	private bool _salvando;

	protected override async Task OnInitializedAsync() {
		var chamado = await ChamadoService.ObterAsync(Id);

		if (chamado is not null) {
			_model = EditarChamadoRequest.De(chamado);
			_statusDisponiveis = [chamado.Status, .. chamado.ProximosStatusPossiveis];
			_responsaveis = await ResponsavelService.ListarAtivosAsync();
		}

		_carregando = false;
	}

	private async Task SalvarAsync() {
		_salvando = true;
		try {
			await ChamadoService.EditarAsync(Id, _model!);
			Snackbar.Add($"Chamado #{Id} atualizado.", Severity.Success);
			Navigation.NavigateTo($"/chamados/{Id}");
		} catch (DomainException ex) {
			Snackbar.Add(ex.Message, Severity.Error);
		} finally {
			_salvando = false;
		}
	}

	private void Voltar() => Navigation.NavigateTo($"/chamados/{Id}");
}
