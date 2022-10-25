using Havit.Data.Patterns.DataSeeds;
using Dominex.Model.Security;

namespace Dominex.DataLayer.Seeds.Core.Security;

public class RoleSeed : DataSeed<CoreProfile>
{
	public override void SeedData()
	{
		var roles = new[]
		{
				new Role()
				{
					Id = (int)Role.Entry.SystemAdministrator,
					Name = nameof(Role.Entry.SystemAdministrator),
					NormalizedName = nameof(Role.Entry.SystemAdministrator).ToUpper(),
				},
				new Role()
				{
					Id = (int)Role.Entry.UserSettingsAdministrator,
					Name = nameof(Role.Entry.UserSettingsAdministrator),
					NormalizedName = nameof(Role.Entry.UserSettingsAdministrator).ToUpper(),
				}
			};

		Seed(For(roles).PairBy(r => r.Id));
	}
}
