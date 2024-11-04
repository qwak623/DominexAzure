using Dominex.Contracts.ServerApi;
using Microsoft.AspNetCore.Components;

namespace Dominex.Web.Client.Components;

public partial class Workspace
{
	[Parameter] public List<string> Cards { get; set; }
	[Inject] protected IGameFacade GameFacade { get; set; }

	private async Task SelectCard(string cardName)
	{
		Cards = (await GameFacade.SelectCard(cardName)).Cards;
	}
}
