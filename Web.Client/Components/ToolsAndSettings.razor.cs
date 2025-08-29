using Dominex.Contracts.Game;
using Dominex.Contracts.Menu;
using Microsoft.AspNetCore.Components;

namespace Dominex.Web.Client.Components;

public partial class ToolsAndSettings
{
	[Parameter] public Func<Task<List<PresetKingdomDto>>> RequestPresetKingdoms { get; set; }
	[Parameter] public Func<List<CardDto>, int, Task<List<CardDto>>> GetRandomCards { get; set; }

	private bool showGenerators = false;
	private bool showAISettings = false;
	private bool showAdvancedSettings = false;

	private List<PresetKingdomDto> PresetKingdoms = new();
	private List<PresetKingdomDto> CustomKingdoms = new();

	private bool showPresetKingdomsSpinner = false;

	private const int RECOMMENDED_CARD_COUNT = 10;

	protected override void OnInitialized()
	{
		State.OnChange += StateHasChanged;
	}

	public void Dispose()
	{
		State.OnChange -= StateHasChanged;
	}

	private void ClickGenerators()
	{
		showGenerators = !showGenerators;
	}

	private void ClickAI()
	{
		showAISettings = !showAISettings;
	}

	private void ClickAdvanced()
	{
		showAdvancedSettings = !showAdvancedSettings;
	}

	private async void ClickRandom()
	{
		State.SetSelectedCards(await GetRandomCards(State.AllCards.ToList(), RECOMMENDED_CARD_COUNT));
	}

	private async void ClickAddRandom()
	{
		if (State.SelectedCards.Count >= RECOMMENDED_CARD_COUNT)
		{
			return;
		}
		var addedCards = await GetRandomCards(State.AvailableCards.ToList(), RECOMMENDED_CARD_COUNT - State.SelectedCards.Count);
		State.AddRangeToSelected(addedCards);

	}

	private void ClickPresetKingdom(PresetKingdomDto kingdom)
	{
		State.SetSelectedCards(kingdom.Cards);
	}

	private async void ClickPresetDropdownButton()
	{
		showPresetKingdomsSpinner = true;
		if (!PresetKingdoms.Any())
		{
			PresetKingdoms = await RequestPresetKingdoms();
		}
		showPresetKingdomsSpinner = false;
		StateHasChanged();
	}
}
