using Dominex.Contracts.Game;
using Microsoft.AspNetCore.Components;

namespace Dominex.Web.Client.Components;

public partial class KingdomSelection
{
	[Parameter] public List<CardDto> AvailableCards { get; set; } = new();
	[Parameter] public List<CardDto> SelectedCards { get; set; } = new();

	private CardDto draggedCard;

	private void OnDragStart(DragEventArgs e, CardDto card)
	{
		draggedCard = card;
	}

	private void OnDrop(DragEventArgs e, List<CardDto> targetColumn)
	{
		if (draggedCard is null)
		{
			return;
		}

		AvailableCards.Remove(draggedCard);
		SelectedCards.Remove(draggedCard);

		targetColumn.Add(draggedCard);

		AvailableCards = AvailableCards.OrderBy(c => c.Name).ToList();
		SelectedCards = SelectedCards.OrderBy(c => c.Name).ToList();

		draggedCard = null;
		StateHasChanged();
	}
}
