using Dominex.Contracts.Game;
using Microsoft.AspNetCore.Components;

namespace Dominex.Web.Client.Components;

public partial class KingdomSelection
{
	[Parameter] public List<CardDto> AvailableCards { get; set; } = new();

	private CardDto draggedCard;

	protected override void OnInitialized()
	{
		State.OnChange += StateHasChanged;
	}

	public void Dispose()
	{
		State.OnChange -= StateHasChanged;
	}

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
		State.SelectedCards.Remove(draggedCard);

		targetColumn.Add(draggedCard);

		AvailableCards = AvailableCards.OrderBy(c => c.Name).ToList();
		State.SelectedCards = State.SelectedCards.OrderBy(c => c.Name).ToList();

		draggedCard = null;
		State.NotifyChanged();
	}
}
