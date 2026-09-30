using Codificar.Application.Chamados;
using Codificar.Application.Responsaveis;
using Codificar.Domain.Enums;
using Codificar.Domain.Extensions;
using Microsoft.AspNetCore.Components;

namespace Codificar.Web.Components.Shared;

public partial class ChamadoForm : ComponentBase {
	private const string AtribuirAutomaticamente = "Atribuir automaticamente";

	[Parameter, EditorRequired]
	public CriarChamadoRequest Model { get; set; } = null!;

	[Parameter, EditorRequired]
	public IReadOnlyList<ResponsavelDto> Responsaveis { get; set; } = [];

	[Parameter]
	public IReadOnlyCollection<StatusChamado> StatusDisponiveis { get; set; } = [];

	[Parameter]
	public bool Salvando { get; set; }

	[Parameter]
	public EventCallback OnSalvar { get; set; }

	[Parameter]
	public EventCallback OnCancelar { get; set; }

	private EditarChamadoRequest? Edicao => Model as EditarChamadoRequest;

	private bool EhEdicao => Edicao is not null;

	private bool PodeReatribuir => Edicao?.Status.EstaPendente() ?? true;

	private string? AjudaResponsavel => (EhEdicao, PodeReatribuir) switch {
		(false, _) => "Automático: vai para quem tem menos chamados em aberto",
		(true, false) => "Reabra o chamado para trocar o responsável",
		_ => null
	};

	private string NomeDoResponsavel(int? id) => id is null
		? AtribuirAutomaticamente
		: Responsaveis.FirstOrDefault(r => r.Id == id)?.Nome ?? string.Empty;
}
