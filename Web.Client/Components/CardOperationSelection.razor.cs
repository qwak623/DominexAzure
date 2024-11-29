using Dominex.Contracts.Game;
using Microsoft.AspNetCore.Components;

namespace Dominex.Web.Client.Components;

public partial class CardOperationSelection
{
	[Parameter] public CardChoiceModel CardChoiceModel { get; set; }
	[Parameter] public CardAnswerModel CardAnswerModel { get; set; }
}
