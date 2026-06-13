using Dominex.Facades.Infrastructure.Security.Claims;
using Dominex.Web.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Dominex.Web.Server.Infrastructure.ConfigurationExtensions;

public static class AuthenticationConfigurationExtension
{
	public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
			.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
			{
				options.LoginPath = AppRoutes.Account.Login;
				options.AccessDeniedPath = AppRoutes.Errors.AccessDenied;
				options.Events.OnSigningIn = HandleSigningInAsync;
			})
			.AddGoogle(googleOptions =>
			{
				googleOptions.ClientId = configuration["Authentication:Google:ClientId"];
				googleOptions.ClientSecret = configuration["Authentication:Google:ClientSecret"];
				googleOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
			});

		return services;
	}

	private static async Task HandleSigningInAsync(CookieSigningInContext context)
	{
		ICustomClaimsBuilder customClaimsBuilder = context.HttpContext.RequestServices.GetRequiredService<ICustomClaimsBuilder>();
		ClaimsPrincipal principal = context.Principal;
		List<Claim> customClaims = await customClaimsBuilder.GetCustomClaimsAsync(principal);

		if (customClaims.Any())
		{
			ClaimsIdentity customClaimsIdentity = new ClaimsIdentity();
			customClaimsIdentity.AddClaims(customClaims);
			principal.AddIdentity(customClaimsIdentity);
		}
	}
}
