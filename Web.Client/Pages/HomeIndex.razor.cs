using Dominex.Contracts.ServerApi;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Dominex.Web.Client.Pages;

public partial class HomeIndex
{
	[Inject] protected IGameFacade GameFacade { get; set; }
	[Inject] protected NavigationManager Navigation { get; set; }

	private async Task ClickStart()
	{
		await GameFacade.Start();

		Navigation.NavigateTo(Routes.Development.Dev);
	}

	private void ClickStartWithChoosingCards()
	{
		Navigation.NavigateTo(Routes.KingdomSelection.Selection);
	}
}
