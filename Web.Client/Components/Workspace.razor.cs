using Dominex.Contracts.Game;
using Dominex.Contracts.ServerApi;
using Microsoft.AspNetCore.Components;

namespace Dominex.Web.Client.Components;

public partial class Workspace
{
	[Parameter] public ChoiceDto Choice { get; set; }
	[Inject] protected IGameFacade GameFacade { get; set; }
	[Inject] protected IHxMessengerService Messenger { get; set; }

	private Answer Answer { get; set; } = new();

	private async Task Submit()
	{
		Answer.Values = Answer.Values.Where(v => v.OperationType != OperationType.Default).ToList();

		if (Answer.Values.Count < Choice.Min)
		{
			Messenger.AddError($"Minimal count of cards with non-default operation is {Choice.Min} but the actual number is {Answer.Values.Count}.");
			return;
		}
		if (Answer.Values.Count > Choice.Max)
		{
			Messenger.AddError($"Maximal count of cards with non-default operation is {Choice.Max} but the actual number is {Answer.Values.Count}.");
			return;
		}

		Choice = await GameFacade.Submit(Answer);
		Answer = new Answer();
	}

	private string GetTitle()
	{
		return Choice?.Message != null ? Choice.Message
			: Choice?.CardPlayed != null ? Choice.CardPlayed.Description
			: Choice?.Type.ToString();
	}
}