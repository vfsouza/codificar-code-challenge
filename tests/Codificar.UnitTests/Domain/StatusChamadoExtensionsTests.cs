using Codificar.Domain.Enums;
using Codificar.Domain.Extensions;

namespace Codificar.UnitTests.Domain;

public class StatusChamadoExtensionsTests {
	[Theory]
	[InlineData(StatusChamado.Aberto, true)]
	[InlineData(StatusChamado.EmAndamento, true)]
	[InlineData(StatusChamado.Resolvido, false)]
	[InlineData(StatusChamado.Fechado, false)]
	public void EstaPendente_ConsideraEmAbertoApenasAbertoEEmAndamento(StatusChamado status, bool esperado) {
		Assert.Equal(esperado, status.EstaPendente());
	}
}
