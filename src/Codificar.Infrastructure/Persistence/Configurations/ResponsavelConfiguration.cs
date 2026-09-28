using Codificar.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codificar.Infrastructure.Persistence.Configurations {
	public class ResponsavelConfiguration {
		public void Configure(EntityTypeBuilder<Responsavel> builder)
		{
			builder.HasKey(r => r.Id);
			builder.Property(r => r.Nome).HasMaxLength(100).IsRequired();
		}
	}
}