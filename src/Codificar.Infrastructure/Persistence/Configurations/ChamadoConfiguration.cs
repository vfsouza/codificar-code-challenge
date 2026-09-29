using Codificar.Domain.Entities;
using Codificar.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codificar.Infrastructure.Persistence.Configurations;

internal sealed class ChamadoConfiguration : IEntityTypeConfiguration<Chamado> {
	public void Configure(EntityTypeBuilder<Chamado> builder) {
		builder.ToTable("Chamados");

		builder.HasKey(c => c.Id);

		builder.Property(c => c.Titulo).HasMaxLength(150).IsRequired();
		builder.Property(c => c.Descricao).HasMaxLength(2000).IsRequired();
		builder.Property(c => c.Solicitante).HasMaxLength(100).IsRequired();

		// Enums como texto: banco legível e imune a reordenação dos membros.
		builder.Property(c => c.Prioridade).HasConversion<string>().HasMaxLength(20);
		builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);

		builder.Property(c => c.AbertoEm).HasConversion(UtcDateTimeConverter.Instancia);
		builder.Property(c => c.AtualizadoEm).HasConversion(UtcDateTimeConverter.Instancia);
		builder.Property(c => c.ConcluidoEm).HasConversion(UtcDateTimeConverter.Instancia);

		builder.HasOne(c => c.Responsavel)
			.WithMany()
			.HasForeignKey(c => c.ResponsavelId)
			.OnDelete(DeleteBehavior.Restrict);

		// Apoiam a contagem de pendentes por responsável e os filtros da listagem.
		builder.HasIndex(c => new { c.ResponsavelId, c.Status });
		builder.HasIndex(c => c.Status);
	}
}
