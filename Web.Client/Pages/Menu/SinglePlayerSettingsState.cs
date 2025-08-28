using Dominex.Contracts.Game;
using Dominex.Contracts.Menu;

namespace Dominex.Web.Client.Pages.Menu;

public class SinglePlayerSettingsState
{
	public List<CardDto> SelectedCards { get; set; } = new();

	public event Action OnChange;
	public void NotifyChanged() => OnChange?.Invoke();
}
