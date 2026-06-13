using System.Linq; // added for LINQ
using System.Security;
using System.Security.Claims;
using Dominex.Contracts.Infrastructure.Security;
using Dominex.DataLayer.Repositories.Security;
using Dominex.Model.Security;
using Dominex.Primitives.Security;
using Dominex.Services.Infrastructure;
using Havit.Data.Patterns.UnitOfWorks;
using Havit.Extensions.DependencyInjection.Abstractions;
using Havit.Services.Caching;

namespace Dominex.Facades.Infrastructure.Security.Claims;

[Service(Profile = ServiceProfiles.WebServer)]
public class CustomClaimsBuilder : ICustomClaimsBuilder
{
	private readonly IUserRepository _userRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly ICacheService _cacheService;

	public CustomClaimsBuilder(
		IUserRepository userRepository,
		IUnitOfWork unitOfWork,
		ICacheService cacheService)
	{
		_userRepository = userRepository;
		_unitOfWork = unitOfWork;
		_cacheService = cacheService;
	}

	public async Task<List<Claim>> GetCustomClaimsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
	{
		// explicit, framework-independent check instead of Contract.Requires
		if (principal?.Identity?.IsAuthenticated != true)
		{
			throw new SecurityException("Principal is not authenticated.");
		}

		// All claims must be created with ClaimConstants.ApplicationIssuer because it is used as a selector by CookieOidcRefresher
		// when creating a new ("refreshed") principal.

		List<Claim> result = new();

		// TODO: replace with real repo lookup when implemented
		User user = null; // await _userRepository.GetByIdentityProviderIdAsync(principal.FindFirst("oid")?.Value, cancellationToken);

		if (user == null)
		{
#if DEBUG
			user = await OnboardFirstUserAsync(principal, cancellationToken);
#endif
			if (user == null)
			{
				throw new SecurityException("User not found.");
			}
		}

		if (user.Disabled)
		{
			throw new SecurityException("User is disabled.");
		}

		if (user.Deleted is not null)
		{
			throw new SecurityException("User is deleted.");
		}

		result.Add(new Claim(ClaimConstants.UserIdClaim, user.Id.ToString(), null, ClaimConstants.ApplicationIssuer));

		var roles = user.UserRoles.Select(ur => (RoleEntry)ur.RoleId);
		foreach (var role in roles)
		{
			result.Add(new Claim(ClaimTypes.Role, role.ToString(), null, ClaimConstants.ApplicationIssuer));
		}

		return result;
	}

	private async Task<User> OnboardFirstUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
	{
		// repository API doesn't expose GetAllAsync in interface; use synchronous method available.
		var all = _userRepository.GetAllIncludingDeleted();
		if (all != null && all.Any())
		{
			return null;
		}

		var user = new User();

		// be defensive when reading claims
		var oid = principal.FindFirst("oid")?.Value;
		if (!string.IsNullOrWhiteSpace(oid))
		{
			user.IdentityProviderExternalId = oid;
		}

		var upn = principal.FindFirst("upn")?.Value ?? principal.FindFirst(ClaimTypes.Upn)?.Value;
		if (!string.IsNullOrWhiteSpace(upn))
		{
			user.Email = upn.Replace("@", "@devmail.");
		}

		user.DisplayName = principal.FindFirst("name")?.Value ?? principal.Identity?.Name;

		// Add all roles for first user in debug
		user.UserRoles.AddRange(Enum.GetValues<RoleEntry>().Select(entry => new UserRole() { RoleId = (int)entry }));

		_unitOfWork.AddForInsert(user);
		await _unitOfWork.CommitAsync(cancellationToken);

		_cacheService.Clear();

		return user;
	}
}