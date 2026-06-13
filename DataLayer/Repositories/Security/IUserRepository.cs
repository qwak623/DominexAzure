using Dominex.Model.Security;
using Dominex.Primitives.Security;

namespace Dominex.DataLayer.Repositories.Security;

public partial interface IUserRepository
{
	// todo toto je nove pres aad, asi nechceme pouzivat
//	Task<User> GetByIdentityProviderIdAsync(string identityProviderId, CancellationToken cancellationToken = default);
	List<User> GetAllIncludingDeleted();
	Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
	Task<User> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
	Task<List<User>> GetUsersInRoleAsync(RoleEntry roleEntry, CancellationToken cancellationToken = default);
}
