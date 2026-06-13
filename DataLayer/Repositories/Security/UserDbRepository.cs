using System.Linq.Expressions;
using Havit.Data.EntityFrameworkCore.Patterns.Repositories;
using Dominex.Model.Security;
using Microsoft.EntityFrameworkCore;
using Dominex.Primitives.Security;

namespace Dominex.DataLayer.Repositories.Security;

public partial class UserDbRepository : IUserRepository
{
	public List<User> GetAllIncludingDeleted()
	{
		return DataIncludingDeleted.Include(GetLoadReferences).ToList();
	}

	public async Task<User> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
	{
		Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(username));

		var normalizedUsername = username.ToUpper();
		return await Data.Include(GetLoadReferences).FirstOrDefaultAsync(u => u.NormalizedUsername == normalizedUsername, cancellationToken);
	}

	public async Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
	{
		Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(email));

		var normalizedEmail = email.ToUpper();
		return await Data.Include(GetLoadReferences).FirstOrDefaultAsync(u => u.NormalizedEmail == email);
	}

	public async Task<List<User>> GetUsersInRoleAsync(RoleEntry roleEntry, CancellationToken cancellationToken = default)
	{
		return await Data.Include(GetLoadReferences).Where(u => u.UserRoles.Any(ur => ur.RoleId == (int)roleEntry)).ToListAsync();
	}

	//public async Task<User> GetByIdentityProviderIdAsync(string identityProviderId, CancellationToken cancellationToken = default)
	//{
	//	Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(identityProviderId));

	//	return await Data.Include(GetLoadReferences).FirstOrDefaultAsync(u => u.IdentityProviderExternalId == identityProviderId, cancellationToken);
	//}

	protected override IEnumerable<Expression<Func<User, object>>> GetLoadReferences()
	{
		yield return (User u) => u.UserRoles;
	}
}
