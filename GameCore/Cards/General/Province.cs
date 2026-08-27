namespace GameCore.Cards.GeneralCards;
public class Province : Card
{
	private static Province province;
	private Province() : base
	(
		type: CardName.Province,
		price: 8,
		addBuys: 0,
		coins: 0,
		isVictory: true,
		isTreasure: false
	)
	{
		province = this;
		VictoryPoints = 6;
	}

	public static Province Get() => province ?? new Province();
}
