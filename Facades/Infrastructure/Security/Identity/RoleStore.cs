using System;
using System.Threading;
using System.Threading.Tasks;
using Dominex.DataLayer.Repositories.Security;
using Dominex.Model.Security;
using Dominex.Primitives.Security;
using Microsoft.AspNetCore.Identity;

namespace Dominex.Facades.Infrastructure.Security.Identity;

public class RoleStore : IRoleStore<Role>
{
	private readonly IRoleRepository roleRepository;

	public RoleStore(IRoleRepository roleRepository)
	{
		this.roleRepository = roleRepository;
	}

	public Task<IdentityResult> CreateAsync(Role role, CancellationToken cancellationToken)
	{
		throw new NotSupportedException();
	}

	public Task<IdentityResult> DeleteAsync(Role role, CancellationToken cancellationToken)
	{
		throw new NotSupportedException();
	}

	public void Dispose()
	{
		// NOOP
	}

	public Task<Role> FindByIdAsync(string roleId, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(roleId))
		{
			throw new ArgumentException("roleId must not be null or whitespace.", nameof(roleId));
		}

		if (!int.TryParse(roleId, out int id))
		{
			throw new ArgumentException("roleId is not a valid integer.", nameof(roleId));
		}

		return Task.FromResult(roleRepository.GetObject(id)); // role is [Cache]d, no need for async
	}

	public Task<Role> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
	{
			if (string.IsNullOrWhiteSpace(normalizedRoleName))
		{
			throw new ArgumentException("normalizedRoleName must not be null or whitespace.", nameof(normalizedRoleName));
		}

		// If Role.Entry enum exists this will work; keep case-insensitive parsing and surface a clear error otherwise.
		try
		{
			var roleEntry = Enum.Parse<RoleEntry>(normalizedRoleName, ignoreCase: true);
			return Task.FromResult(roleRepository.GetObject((int)roleEntry)); // role is [Cache]d, no need for async
		}
		catch (ArgumentException)
		{
			throw new ArgumentException($"Unknown role name '{normalizedRoleName}'.", nameof(normalizedRoleName));
		}
	}

	public Task<string> GetNormalizedRoleNameAsync(Role role, CancellationToken cancellationToken)
	{
		if (role is null)
		{
			throw new ArgumentNullException(nameof(role));
		}

		// Role type in this repo doesn't show NormalizedName property in the snippet.
		// Return an invariant uppercase form of Name so Identity's expectations are satisfied.
		return Task.FromResult(role.Name?.ToUpperInvariant());
	}

	public Task<string> GetRoleIdAsync(Role role, CancellationToken cancellationToken)
	{
		if (role is null)
		{
			throw new ArgumentNullException(nameof(role));
		}

		return Task.FromResult(role.Id.ToString());
	}

	public Task<string> GetRoleNameAsync(Role role, CancellationToken cancellationToken)
	{
		if (role is null)
		{
			throw new ArgumentNullException(nameof(role));
		}

		return Task.FromResult(role.Name);
	}

	public Task SetNormalizedRoleNameAsync(Role role, string normalizedName, CancellationToken cancellationToken)
	{
		throw new NotSupportedException();
	}

	public Task SetRoleNameAsync(Role role, string roleName, CancellationToken cancellationToken)
	{
		throw new NotSupportedException();
	}

	public Task<IdentityResult> UpdateAsync(Role role, CancellationToken cancellationToken)
	{
		throw new NotSupportedException();
	}
}
