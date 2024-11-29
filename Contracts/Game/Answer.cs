namespace Dominex.Contracts.Game;
public class Answer
{
	public List<CardAnswerModel> Values { get; set; } = new();
}

public class CardAnswerModel
{
	public int Index { get; set; }
	public OperationType OperationType { get; set; }
}