using Dominex.Contracts;
using Dominex.Contracts.Game;
using Dominex.Contracts.ServerApi;
using Microsoft.AspNetCore.Components;

namespace Dominex.Web.Client.Pages.Development;

public partial class KingdomSelection
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

	private void ClickAddToSelected(int index)
	{
		SelectedCards.Add(AvailableCards[index]);
		AvailableCards.RemoveAt(index);
	}
	private void ClickRemoveFromSelected(int index)
	{
		AvailableCards.Add(SelectedCards[index]);
		SelectedCards.RemoveAt(index);
	}
}
