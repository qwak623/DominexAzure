namespace GameCore.Cards.Prosperity;

public class Platinum : Card
{
	private static Platinum platinum;
	private Platinum() : base(CardType.Treasure)
	{
		Name = CardName.Platinum;
		DefaultPrice = 9;
		Coins = 5;
		platinum = this;
	}

	public override int GetCountInKingdomPile(int playerCount) => 12;

	public static Platinum Get() => platinum ?? new Platinum();
}
