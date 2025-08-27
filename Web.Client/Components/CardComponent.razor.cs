using Dominex.Contracts.Game;
using Microsoft.AspNetCore.Components;

namespace Dominex.Web.Client.Components;

public partial class CardComponent
{
	[Parameter] public CardDto Card { get; set; }

	private CardTooltip cardTooltip;

	public void HideTooltip()
	{
		cardTooltip.HideToolTip();
	}
}
