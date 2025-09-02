using Dominex.Contracts.Game;

namespace Dominex.Contracts.Results;

public class CardResultsDto
{
	public CardDto Card { get; set; }
	public int PointsPerCard { get; set; }
	public int Count { get; set; }
}
