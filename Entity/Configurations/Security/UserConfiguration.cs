using Dominex.Model.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dominex.Entity.Configurations.Security;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
	public void Configure(EntityTypeBuilder<User> builder)
	{
		//builder.HasIndex(user => user.IdentityProviderExternalId).HasFilter("Deleted IS NULL").IsUnique();
		builder.HasIndex(user => user.NormalizedUsername).HasFilter("Deleted IS NULL").IsUnique();
		builder.HasIndex(user => user.NormalizedEmail).HasFilter("Deleted IS NULL").IsUnique();
	}
}