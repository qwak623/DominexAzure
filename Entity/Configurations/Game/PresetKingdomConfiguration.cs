using System.Text.Json;
using Dominex.Model.Game;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dominex.Entity.Configurations.Game;

public class PresetKingdomConfiguration : IEntityTypeConfiguration<PresetKingdom>
{
	public void Configure(EntityTypeBuilder<PresetKingdom> builder)
	{
		builder.Property(p => p.Cards)
			.HasConversion(
				v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
				v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null))
			.HasColumnType("nvarchar(max)");

		builder.HasIndex(p => p.Name).IsUnique();
	}
}
