using Dominex.Contracts.Game;
using Microsoft.AspNetCore.Components;

namespace Dominex.Web.Client.Components;

public partial class CardComponent
{
	[Parameter] public CardDto Card { get; set; }
	[Parameter] public int? CardCount { get; set; } = null;

	private CardTooltip cardTooltip;

	public void HideTooltip()
	{
		cardTooltip.HideToolTip();
	}

	private string GetStyle(CardDto card)
	{
		string actionColor = "#f5f2ed";
		string curseColor = "#c1a1ca";
		string reactionColor = "#a4cdec";
		string treasureColor = "#f0e29c";
		string victoryColor = "#c0daad";

		string oneColorStyle = "background-color: {0}";

		if (card.Name == "Curse")
		{
			return string.Format(oneColorStyle, curseColor);
		}
		if (card.IsAction && !card.IsReaction && !card.IsTreasure && !card.IsVictory)
		{
			return string.Format(oneColorStyle, actionColor);
		}
		if (card.IsReaction && !card.IsTreasure && !card.IsVictory)
		{
			return string.Format(oneColorStyle, reactionColor);
		}
		if (!card.IsAction && !card.IsReaction && card.IsTreasure && !card.IsVictory)
		{
			return string.Format(oneColorStyle, treasureColor);
		}
		if (!card.IsAction && !card.IsReaction && !card.IsTreasure && card.IsVictory)
		{
			return string.Format(oneColorStyle, victoryColor);
		}
		return string.Format(oneColorStyle, "#0c0b0e");
	}
}
