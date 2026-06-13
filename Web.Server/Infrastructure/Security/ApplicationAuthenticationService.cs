using System.Security.Claims;
using Dominex.Contracts.Infrastructure.Security;
using Dominex.DataLayer.Repositories.Security;
using Dominex.Facades.Infrastructure.Security.Authentication;
using Dominex.Model.Security;

namespace Dominex.Web.Server.Infrastructure.Security;

public class ApplicationAuthenticationService : IApplicationAuthenticationService
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	private readonly Lazy<User> _userLazy;

	public ApplicationAuthenticationService(IHttpContextAccessor httpContextAccessor, IUserRepository userRepository)
	{
		_httpContextAccessor = httpContextAccessor;

		_userLazy = new Lazy<User>(() => userRepository.GetObject(GetCurrentUserId()));
	}

	public ClaimsPrincipal GetCurrentClaimsPrincipal()
	{
		return _httpContextAccessor.HttpContext.User;
	}

	public User GetCurrentUser() => _userLazy.Value;

	public int GetCurrentUserId()
	{
		var principal = GetCurrentClaimsPrincipal();
		Claim userIdClaim = principal.Claims.Single(claim => (claim.Type == ClaimConstants.UserIdClaim));
		return Int32.Parse(userIdClaim.Value);
	}
}
