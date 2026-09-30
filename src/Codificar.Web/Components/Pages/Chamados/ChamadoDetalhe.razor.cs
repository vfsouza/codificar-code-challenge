using Codificar.Application.Chamados;
using Codificar.Domain.Enums;
using Codificar.Domain.Exceptions;
using Codificar.Domain.Extensions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Codificar.Web.Components.Pages.Chamados;

public partial class ChamadoDetalhe : ComponentBase {
	[Inject] private IChamadoService ChamadoService { get; set; } = null!;
	[Inject] private ISnackbar Snackbar { get; set; } = null!;

	[Parameter]
	public int Id { get; set; }

	private ChamadoDetalheDto? _chamado;
	private bool _carregando = true;
	private bool _salvando;

	protected override async Task OnInitializedAsync() {
		_chamado = await ChamadoService.ObterAsync(Id);
		_carregando = false;
	}

	private async Task AlterarStatusAsync(StatusChamado novoStatus) {
		_salvando = true;
		try {
			await ChamadoService.AlterarStatusAsync(Id, novoStatus);
			_chamado = await ChamadoService.ObterAsync(Id);
			Snackbar.Add($"Status alterado para \"{novoStatus.Descricao()}\".", Severity.Success);
		} catch (DomainException ex) {
			Snackbar.Add(ex.Message, Severity.Error);
		} finally {
			_salvando = false;
		}
	}

	private static string RotuloDaAcao(StatusChamado atual, StatusChamado destino) => destino switch {
		StatusChamado.EmAndamento when atual == StatusChamado.Resolvido => "Reabrir",
		StatusChamado.EmAndamento => "Iniciar atendimento",
		StatusChamado.Resolvido => "Marcar como resolvido",
		StatusChamado.Fechado => "Fechar",
		_ => destino.Descricao()
	};
}
