namespace Dominex.Web.Client.Components;

public partial class ToolsAndSettings
{
	private bool showGenerators = false;
	private bool showAISettings = false;
	private bool showAdvancedSettings = false;

	private List<string> PresetGames = new();
	private List<string> CustomGames = new();

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

	private void ClickCustomGame(string game)
	{

	}

	private void ClickPresetGame(string game)
	{

	}
}
