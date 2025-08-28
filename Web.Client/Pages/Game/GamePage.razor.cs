using Dominex.Contracts;
using Dominex.Contracts.Game;
using Dominex.Contracts.ServerApi;
using Microsoft.AspNetCore.Components;

namespace Dominex.Web.Client.Pages.Game;

public partial class GamePage
{
	[Inject] protected IGameFacade GameFacade { get; set; }
	//[Inject] protected NavigationManager Navigation { get; set; }

	private ChoiceDto Choice { get; set; }

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();

		Choice = await GameFacade?.JoinGame(/*Dto.FromValue(new Guid()), */Dto.FromValue(1));
	}

	private async Task Click()
	{
		// todo cards nebudou na této stránce

		//Navigation.NavigateTo(Routes.Development.Dev);
	}
}
