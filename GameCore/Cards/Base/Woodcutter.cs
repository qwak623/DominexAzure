namespace GameCore.Cards.Base;

public class Woodcutter : Card
{
	private static Woodcutter woodcutter;
	private Woodcutter() : base(CardType.Action)
	{
		Name = CardName.Woodcutter;
		DefaultPrice = 3;
		AddBuys = 1;
		AddCoins = 2;
		woodcutter = this;
	}

	public static Woodcutter Get() => woodcutter ?? new Woodcutter();
}
