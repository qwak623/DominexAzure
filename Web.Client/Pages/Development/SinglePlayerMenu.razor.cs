using Dominex.Contracts.Game;
using Dominex.Contracts.ServerApi;
using Microsoft.AspNetCore.Components;

namespace Dominex.Web.Client.Pages.Development;

public partial class SinglePlayerMenu
{
	[Inject] protected IGameFacade GameFacade { get; set; }
	[Inject] protected NavigationManager Navigation { get; set; }

	private List<CardDto> AvailableCards { get; set; }
	private List<CardDto> SelectedCards { get; set; }

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();

		AvailableCards = await GameFacade.RequestAvailableCards();
		SelectedCards = new List<CardDto>();
	}

	private async Task ClickStart()
	{
		await GameFacade.StartWithCards(SelectedCards.Select(c => c.Type));

		Navigation.NavigateTo(Routes.Development.Dev);
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
