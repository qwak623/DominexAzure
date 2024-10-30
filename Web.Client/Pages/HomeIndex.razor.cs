using Dominex.Contracts.ServerApi;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Dominex.Web.Client.Pages;

public partial class HomeIndex
{
	[Inject] protected IGameFacade GameFacade { get; set; }
	[Inject] protected NavigationManager Navigation { get; set; }

	//private List<string> Cards { get; set; } = new List<string>();

	private async Task Click()
	{
		await GameFacade.Start();

		// todo cards nebudou na této stránce

		Navigation.NavigateTo(Routes.Development.Dev);
	}

	//private async Task SelectCard(string cardName)
	//{
	//	Cards = (await GameFacade.SelectCard(cardName)).Cards;
	//}
}
