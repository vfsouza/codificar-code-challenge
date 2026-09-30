namespace Codificar.UnitTests.Helpers;

public sealed class RelogioFixo(DateTime agoraUtc) : TimeProvider {
	public override DateTimeOffset GetUtcNow() => new(agoraUtc);
}
