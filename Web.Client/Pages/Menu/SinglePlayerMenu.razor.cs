using Dominex.Contracts.Game;
using Dominex.Contracts.Menu;
using Dominex.Contracts.ServerApi;
using Microsoft.AspNetCore.Components;

namespace Dominex.Web.Client.Pages.Menu;

public partial class SinglePlayerMenu
{
	[Inject] protected IGameFacade GameFacade { get; set; }
	[Inject] protected IGameSetupFacade GameSetupFacade { get; set; }
	[Inject] protected NavigationManager Navigation { get; set; }

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();

		State.SetAvailableCards(await GameSetupFacade.RequestAvailableCards());
	}

	private async Task ClickStart()
	{
		await GameFacade.StartWithCards(new StartWithCardsRequest
		{
			CardTypes = State.SelectedCards.Select(c => c.Type).ToList(),
			AddColonyAndPlatinum = State.AddColonyAndPlatinum,
		});

		Navigation.NavigateTo(AppRoutes.Game.GamePage);
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

	private async Task<List<PresetKingdomDto>> RequestPresetKingdoms()
	{
		return await GameSetupFacade.RequestPresetKingdoms();
	}

	private async Task<List<CardDto>> GetRandomCards(List<CardDto> availableCards, int count)
	{
		return (await GameSetupFacade.GetRandomCards(new GetRandomCardsRequest { AvailableCards = availableCards, Count = count })).Cards;
	}
}
