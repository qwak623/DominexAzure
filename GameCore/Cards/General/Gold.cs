namespace GameCore.Cards.GeneralCards;
public class Gold : Card
{
	private static Gold gold;
	private Gold() : base(CardType.Treasure)
	{
		Name = CardName.Gold;
		DefaultPrice = 6;
		Coins = 3;
		gold = this;
	}

	public static Gold Get() => gold ?? new Gold();
}
