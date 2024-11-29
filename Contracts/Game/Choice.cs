namespace Dominex.Contracts.Game;
public class Choice
{
	//	public State State { get; set; }
	public ChoiceType Type { get; set; }

	// TODO asi se bude muset trochu změnit, není jasné, co je vlastně selection
	public int MinNumberOfSelections { get; set; }
	public int MaxNumberOfSelections { get; set; }
	public List<CardChoiceModel> Values { get; set; } = new();

	public Choice() { }

	public Choice(ChoiceType type, int minNumberOfSelections, int maxNumberOfSelections, IEnumerable<CardDto> cards, List<OperationType> operations)
	{
		Type = type;
		MinNumberOfSelections = minNumberOfSelections;
		MaxNumberOfSelections = maxNumberOfSelections;
		Values = cards.Select(c => new CardChoiceModel(c, operations)).ToList();
	}
}

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

// todo do jiných souboru (nebo do třídy choice) uvidime
public enum ChoiceType
{
	Buy,
	Play,
	SpyDiscard,
	MilitiaDiscard,
	RemodelTrash,
	ChapelTrash,
	BureaucratPutOnTop,
	CellarDiscard,
	ChancellorDiscard,
	LibrarySkip,
	ThiefChoose,
	ThiefSteal,
	ThroneRoomPlay,
	MineTrash,
}

public enum OperationType
{
	Default,
	Buy,
	Trash,
	Discard,
	Steal,
	Play,
	PutOnTop,
	Skip,
	Choose,
}
