using Codificar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codificar.Infrastructure.Persistence.Configurations;

internal sealed class ResponsavelConfiguration : IEntityTypeConfiguration<Responsavel> {
	public void Configure(EntityTypeBuilder<Responsavel> builder) {
		builder.ToTable("Responsaveis");

		builder.HasKey(r => r.Id);
		builder.Property(r => r.Nome).HasMaxLength(100).IsRequired();

		builder.HasData(
			new { Id = 1, Nome = "Ana Souza", Ativo = true },
			new { Id = 2, Nome = "Bruno Lima", Ativo = true },
			new { Id = 3, Nome = "Carla Mendes", Ativo = true },
			new { Id = 4, Nome = "Diego Rocha", Ativo = true });
	}
}
