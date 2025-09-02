using Dominex.Contracts;
using Dominex.Contracts.Game;
using Dominex.Contracts.Results;
using Dominex.Contracts.ServerApi;
using Microsoft.AspNetCore.Components;

namespace Dominex.Web.Client.Pages.Game;

public partial class Results
{
	[Inject] protected IGameFacade GameFacade { get; set; }

	private GameResultsDto GameResults { get; set; }

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();

		GameResults = await GameFacade?.GetGameResults();
	}
}
