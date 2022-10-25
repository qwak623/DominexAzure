namespace GameCore.Cards.GeneralCards;

public class Curse : Card
{
	private static Curse curse;
	private Curse() : base
	(
		name: "Curse",
		type: CardType.Curse,
		price: 0,
		addBuys: 0,
		victoryPoints: -1,
		coins: 0,
		isVictory: true,
		isTreasure: false
	) => curse = this;

	public static Curse Get() => curse ?? new Curse();
}
