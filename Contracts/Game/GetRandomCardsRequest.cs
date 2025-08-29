namespace Dominex.Contracts.Game;

public class GetRandomCardsRequest
{
	public List<CardDto> AvailableCards { get; set; }
	public int Count { get; set; }
}
