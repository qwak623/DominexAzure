namespace GameCore.Cards.GeneralCards;
public class Copper : Card
{
	private static Copper copper;
	private Copper() : base
	(
		name: "Copper",
		type: CardType.Copper,
		price: 0,
		addBuys: 0,
		victoryPoints: 0,
		coins: 1,
		isVictory: false,
		isTreasure: true
	) => copper = this;

	public static Copper Get() => copper ?? new Copper();
}
