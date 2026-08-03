namespace GameCore.Cards;

public class CardInstance
{
	public int Id { get; init; }
	public Card Card { get; init; }
	public Pile Pile { get; internal set; }

	public CardInstance(Card card, Pile pile, int id)
	{
		Id = id;
		Card = card;
		Pile = pile;
	}

	/// <summary>
	/// Special action card effect including adding actions etc.
	/// </summary>
	/// <param name="player"></param>
	public void WhenPlayAction(IPlayer player) => Card.WhenPlayAction(player, this);

	public void WhenPlayTreasure(IPlayer player) => Card.WhenPlayTreasure(player);

	public bool IsVictory => Card.IsVictory;
	public bool IsTreasure => Card.IsTreasure;
	public bool IsAction => Card.IsAction;
	public bool IsReaction => Card.IsReaction;
	public bool IsAttack => Card.IsAttack;
	public string Name => Card.Name;


	public bool Equals(CardInstance other) => Id == other.Id;

	public override int GetHashCode() => Id.GetHashCode();

	public override string ToString() => $"{Name} (id: {Id})";
}
