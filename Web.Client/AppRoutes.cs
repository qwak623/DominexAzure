namespace Dominex.Web.Client;

public static class AppRoutes
{
	public const string Home = "/";

	public static class Administration
	{
		public const string Index = "/admin/";
	}

	public static class UserAdministration
	{
		public const string PageName = "/admin/user/page-name";
	}

	public static class Diagnostics
	{
		public const string Info = "/diag/info";
	}

	public static class Game
	{
		public const string GamePage = "/game";
		public const string Results = "/game/results";
	}

	public static class Menu
	{
		public const string SinglePlayer = "/kingdom-selection";
	}

	public static class Errors
	{
		public const string AccessDenied = "/access-denied";
	}

	public static class Account
	{
		public const string Login = "/login";
	}
}
