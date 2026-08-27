namespace GameCore.Cards.Base;
public class Market : Card
{
	private static Market market;
	private Market() : base(CardType.Action)
	{
		Name = CardName.Market;
		DefaultPrice = 5;
		AddActions = 1;
		AddBuys = 1;
		AddCoins = 1;
		DrawCards = 1;
		market = this;
	}

	public static Market Get() => market ?? new Market();
}
