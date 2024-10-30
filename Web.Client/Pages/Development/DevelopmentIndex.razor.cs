using Dominex.Contracts;
using Dominex.Contracts.Game;
using Dominex.Contracts.ServerApi;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Org.BouncyCastle.Tls;

namespace Dominex.Web.Client.Pages.Development;

public partial class DevelopmentIndex
{
	[Inject] protected IGameFacade GameFacade { get; set; }
	//[Inject] protected NavigationManager Navigation { get; set; }

	private List<string> Cards { get; set; } = new List<string>();
	private InfoDto Info { get; set; }
	private List<string> Log { get; set; }

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();

		var message = await GameFacade?.JoinGame(/*Dto.FromValue(new Guid()), */Dto.FromValue(1));
		Cards = message.Choice.Cards;
		Info = message.Info;
	}

	private async Task Click()
	{

		// todo cards nebudou na této stránce

		//Navigation.NavigateTo(Routes.Development.Dev);
	}

	private async Task SelectCard(string cardName)
	{
		Cards = (await GameFacade.SelectCard(cardName)).Cards;
	}
}
