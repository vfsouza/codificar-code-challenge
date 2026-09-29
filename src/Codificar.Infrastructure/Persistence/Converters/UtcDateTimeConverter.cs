using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Codificar.Infrastructure.Persistence.Converters;

/// <summary>
/// O SQLite não guarda o Kind do DateTime; na leitura, marca os valores como UTC.
/// Também funciona em propriedades DateTime? (o EF não repassa null ao converter).
/// </summary>
internal sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
	gravar => gravar.Kind == DateTimeKind.Local ? gravar.ToUniversalTime() : DateTime.SpecifyKind(gravar, DateTimeKind.Utc),
	ler => DateTime.SpecifyKind(ler, DateTimeKind.Utc)) {
	public static readonly UtcDateTimeConverter Instancia = new();
}
