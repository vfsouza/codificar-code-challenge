using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Codificar.Infrastructure.Persistence.Converters;

// O SQLite não guarda o DateTimeKind
internal sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
	gravar => gravar.Kind == DateTimeKind.Local ? gravar.ToUniversalTime() : DateTime.SpecifyKind(gravar, DateTimeKind.Utc),
	ler => DateTime.SpecifyKind(ler, DateTimeKind.Utc)) {
	public static readonly UtcDateTimeConverter Instancia = new();
}
