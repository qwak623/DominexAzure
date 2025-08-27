namespace Dominex.Web.Client;

public static class Routes
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

	public static class Development
	{
		public const string Dev = "/development";
	}

	public static class Menu
	{
		public const string SinglePlayer = "/kingdom-selection";
	}
}
