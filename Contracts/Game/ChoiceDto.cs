namespace Dominex.Contracts.Game;
public class ChoiceDto
{
	public ChoiceType Type { get; set; }
	public CardDto CardPlayed { get; set; }
	public string Message { get; set; }

	/// <summary>
	/// Minimal number of cards with non-default operation selected.
	/// </summary>
	public int Min { get; set; }

	/// <summary>
	/// Maximal number of cards with non-default operation selected.
	/// </summary>
	public int Max { get; set; }
	public List<CardChoiceModel> Values { get; set; } = new();

	public ChoiceDto() { }

	public ChoiceDto(CardDto cardPlayed, ChoiceType type, int min, int max, IEnumerable<CardDto> cards, List<OperationType> operations, string message = null)
	{
		CardPlayed = cardPlayed;
		Type = type;
		Min = min;
		Max = max;
		Values = cards.Select(c => new CardChoiceModel(c, operations)).ToList();
		Message = message;
	}
}