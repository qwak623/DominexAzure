namespace GameCore.Cards.GeneralCards;
public class Province : Card
{
	private static Province province;
	private Province() : base
	(
		name: "Province",
		type: CardType.Province,
		price: 8,
		addBuys: 0,
		victoryPoints: 6,
		coins: 0,
		isVictory: true,
		isTreasure: false
	) => province = this;

	public static Province Get() => province ?? new Province();
}
