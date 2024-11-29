using System.Text;
using Dominex.Contracts.Game;
using Microsoft.AspNetCore.Components;

namespace Dominex.Web.Client.Components;

public partial class CardTooltip
{
	[Parameter] public CardDto CardDto { get; set; }
	[Parameter] public RenderFragment ChildContent { get; set; }

	private string TooltipText()
	{
		StringBuilder sb = new();

		TooltipAddLine(sb, "VP", CardDto.VictoryPoints, addPlural: false);
		TooltipAddLine(sb, "Coin", CardDto.Coins, addPlural: true);
		TooltipAddLine(sb, "Card", CardDto.DrawCards, addPlural: true);
		TooltipAddLine(sb, "Action", CardDto.AddActions, addPlural: true);
		TooltipAddLine(sb, "Buy", CardDto.AddBuys, addPlural: true);
		TooltipAddLine(sb, "$", CardDto.AddCoins, addPlural: false);

		sb.Append(CardDto.Description);
		return sb.ToString();
	}

	private void TooltipAddLine(StringBuilder sb, string text, int count, bool addPlural)
	{
		if (count > 0)
		{
			sb.Append($"<b>+{count} {text}{((addPlural && count > 1) ? 's' : "")}</b><br>");
		}
	}
}
