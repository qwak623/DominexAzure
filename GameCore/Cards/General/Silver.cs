namespace GameCore.Cards.GeneralCards;
public class Silver : Card
{
	private static Silver silver;
	private Silver() : base
	(
		name: "Silver",
		type: CardType.Silver,
		price: 3,
		addBuys: 0,
		coins: 2,
		isVictory: false,
		isTreasure: true
	) => silver = this;

	public static Silver Get() => silver ?? new Silver();
}
