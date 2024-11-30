using Dominex.Contracts.Game;
using Dominex.Contracts.ServerApi;
using Microsoft.AspNetCore.Components;

namespace Dominex.Web.Client.Components;

public partial class Workspace
{
	[Parameter] public ChoiceDto Choice { get; set; }
	[Inject] protected IGameFacade GameFacade { get; set; }

	private Answer Answer { get; set; } = new();

	private async Task Submit()
	{
		Answer.Values = Answer.Values.Where(v => v.OperationType != OperationType.Default).ToList();

		// TODO kontrola podmínek
		Choice = await GameFacade.Submit(Answer);
		Answer = new Answer();
	}
}