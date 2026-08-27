namespace GameCore.Cards.GeneralCards;
public class Gold : Card
{
	private static Gold gold;
	private Gold() : base
	(
		type: CardName.Gold,
		price: 6,
		addBuys: 0,
		coins: 3,
		isVictory: false,
		isTreasure: true
	) => gold = this;

	public static Gold Get() => gold ?? new Gold();
}
