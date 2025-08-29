using Dominex.Contracts.Game;

namespace Dominex.Web.Client.Components;

public partial class KingdomSelection
{
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

	private void OnDropToSelected(DragEventArgs e)
	{
		if (draggedCard is not null)
		{
			State.AddCardToSelected(draggedCard);
		}
	}

	private void OnDropToAvailable(DragEventArgs e)
	{
		if (draggedCard is not null)
		{
			State.AddCardToAvailable(draggedCard);
		}
	}
}
