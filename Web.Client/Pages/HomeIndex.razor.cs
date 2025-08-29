using Microsoft.AspNetCore.Components;

namespace Dominex.Web.Client.Pages;

public partial class HomeIndex
{
	[Inject] protected NavigationManager Navigation { get; set; }

	private void ClickSinglePlayer()
	{
		Navigation.NavigateTo(Routes.Menu.SinglePlayer);
	}

	private void ClickMultiplayer()
	{
		//TODO
	}

	private void ClickInfo()
	{
		//TODO
	}

	private void ClickProfile()
	{
		//TODO
	}
}
