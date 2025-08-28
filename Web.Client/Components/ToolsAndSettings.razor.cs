using Dominex.Contracts.Menu;
using Microsoft.AspNetCore.Components;

namespace Dominex.Web.Client.Components;

public partial class ToolsAndSettings
{
	[Parameter] public Func<Task<List<PresetKingdomDto>>> RequestPresetKingdoms { get; set; }

	private bool showGenerators = false;
	private bool showAISettings = false;
	private bool showAdvancedSettings = false;

	private List<PresetKingdomDto> PresetKingdoms = new();
	private List<PresetKingdomDto> CustomKingdoms = new();

	private bool showPresetKingdomsSpinner = false;

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

	private void ClickRandom()
	{

	}

	private void ClickAddRandom()
	{

	}

	private void ClickCustomKingdom(PresetKingdomDto kingdom)
	{

	}

	private void ClickPresetKingdom(PresetKingdomDto kingdom)
	{
		State.SelectedCards = kingdom.Cards;
		State.NotifyChanged();
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
