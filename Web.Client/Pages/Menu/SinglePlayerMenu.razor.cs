using Dominex.Contracts.Game;
using Dominex.Contracts.ServerApi;
using Microsoft.AspNetCore.Components;

namespace Dominex.Web.Client.Pages.Menu;

public partial class SinglePlayerMenu
{
	[Inject] protected IGameFacade GameFacade { get; set; }
	[Inject] protected NavigationManager Navigation { get; set; }

	private List<CardDto> AvailableCards { get; set; }

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();

		AvailableCards = await GameFacade.RequestAvailableCards();
	}

	private async Task ClickStart()
	{
		await GameFacade.StartWithCards(State.SelectedCards.Select(c => c.Type));

		Navigation.NavigateTo(Routes.Game.GamePage);
	}

	private void ClickSaveSettings()
	{

	}

	private void ClickLoadSettings()
	{

	}

	private void ClickLoadGame()
	{

	}
}
