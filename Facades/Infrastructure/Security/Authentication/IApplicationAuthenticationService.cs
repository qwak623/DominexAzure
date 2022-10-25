using System.Security.Claims;
using Dominex.Model.Security;

namespace Dominex.Facades.Infrastructure.Security.Authentication;

/// <summary>
/// Vrací aktuálně přihlášeného uživatele jako ClaimsPrincipal nebo LoginAccount.
/// Implementace interface ve Web.Server.
/// </summary>
public interface IApplicationAuthenticationService
{
	ClaimsPrincipal GetCurrentClaimsPrincipal();
	User GetCurrentUser();
}
