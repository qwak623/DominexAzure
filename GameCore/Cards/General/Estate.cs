namespace GameCore.Cards.GeneralCards;

public class Estate : Card
{
	private static Estate estate;
	private Estate() : base
	(
		name: "Estate",
		type: CardType.Estate,
		price: 2,
		addBuys: 0,
		victoryPoints: 1,
		coins: 0,
		isVictory: true,
		isTreasure: false
	) => estate = this;

	public static Estate Get() => estate ?? new Estate();
}
