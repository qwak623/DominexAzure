using Dominex.Contracts.Game;
using Microsoft.AspNetCore.Components;

namespace Dominex.Web.Client.Components;

public partial class DraggableCard
{
	[Parameter] public CardDto Card { get; set; }
	[Parameter] public Action<DragEventArgs> OnDragStart { get; set; }

	private CardComponent cardComponent;

	private void HandleDragStart(DragEventArgs e)
	{
		cardComponent.HideTooltip();
		OnDragStart(e);
	}
}
