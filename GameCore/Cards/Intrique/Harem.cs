namespace GameCore.Cards.Intrique;
public class Harem : Card
{
	private static Harem harem;
	private Harem() : base([CardType.Victory, CardType.Treasure])
	{
		Name = CardName.Harem;
		DefaultPrice = 6;
		Coins = 2;
		harem = this;
		VictoryPoints = 2;
	}

	public static Harem Get() => harem ?? new Harem();
}
