using Dominex.Contracts.Game;
using Microsoft.AspNetCore.Components;
using MimeKit;

namespace Dominex.Web.Client.Components;

public partial class CardOperationSelection
{
	[Parameter] public CardChoiceModel CardChoiceModel { get; set; }
	[Parameter] public CardAnswerModel CardAnswerModel { get; set; }
	[Parameter] public int CardIndex { get; set; }
}
