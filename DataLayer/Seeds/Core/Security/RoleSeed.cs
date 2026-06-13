using Havit.Data.Patterns.DataSeeds;
using Dominex.Model.Security;
using Dominex.Primitives.Security;
using System.Threading.Tasks;

namespace Dominex.DataLayer.Seeds.Core.Security;

public class RoleSeed : DataSeed<CoreProfile>
{
	public override async Task SeedDataAsync(CancellationToken cancellationToken)
	{
		var roles = Enum.GetValues<RoleEntry>().Select(entry => new Role { Id = (int)entry, Name = entry.ToString() }).ToArray();

		await SeedAsync(For(roles).PairBy(r => r.Id), cancellationToken);
	}
}
