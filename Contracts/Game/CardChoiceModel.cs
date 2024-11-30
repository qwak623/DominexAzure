namespace Dominex.Contracts.Game;

public class CardChoiceModel
{
	public CardDto Card { get; set; }
	public List<OperationType> Operations { get; set; }

	public CardChoiceModel() { }
	public CardChoiceModel(CardDto card, List<OperationType> operations)
	{
		Card = card;
		Operations = operations;
	}
}
