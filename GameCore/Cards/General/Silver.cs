namespace GameCore.Cards.GeneralCards;
public class Silver : Card
{
	private static Silver silver;
	private Silver() : base(CardType.Treasure)
	{
		Name = CardName.Silver;
		DefaultPrice = 3;
		Coins = 2;
		silver = this;
	}

	public static Silver Get() => silver ?? new Silver();
}
