using Dominex.Contracts.Game;
using Microsoft.AspNetCore.Components;

namespace Dominex.Web.Client.Components;

public partial class KingdomSelection
{
	[Parameter] public List<CardDto> AvailableCards { get; set; }
	[Parameter] public List<CardDto> SelectedCards { get; set; }

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
