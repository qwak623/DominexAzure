using System.Collections.Immutable;
using Dominex.Contracts.Game;

namespace Dominex.Web.Client.Pages.Menu;

public class SinglePlayerSettingsState
{
	private List<(CardDto Card, bool Selected)> allCards = new();

	public ImmutableList<CardDto> AvailableCards => allCards.Where(c => !c.Selected).Select(c => c.Card).ToImmutableList();
	public ImmutableList<CardDto> SelectedCards => allCards.Where(c => c.Selected).Select(c => c.Card).ToImmutableList();
	public ImmutableList<CardDto> AllCards => allCards.Select(c => c.Card).ToImmutableList();

	public event Action OnChange;
	public void NotifyChanged() => OnChange?.Invoke();

	public void SetAvailableCards(List<CardDto> cards)
	{
		allCards = cards.OrderBy(c => c.Name).Select(c => (c, false)).ToList();
		NotifyChanged();
	}

	public void SetSelectedCards(List<CardDto> cards)
	{
		allCards = allCards.Select(c => (c.Card, cards.Contains(c.Card))).ToList();
		NotifyChanged();
	}

	public void AddCardToAvailable(CardDto card)
	{
		allCards = allCards.Select(c => (c.Card == card) ? (c.Card, false) : c).ToList();
		NotifyChanged();
	}

	public void AddCardToSelected(CardDto card)
	{
		allCards = allCards.Select(c => (c.Card == card) ? (c.Card, true) : c).ToList();
		NotifyChanged();
	}

	public void AddRangeToSelected(IEnumerable<CardDto> cards)
	{
		allCards = allCards.Select(c => (cards.Contains(c.Card)) ? (c.Card, true) : c).ToList();
		NotifyChanged();
	}
}
