namespace GameCore.Cards.Base;
public class Festival : Card
{
	private static Festival festival;
	private Festival() : base(CardType.Action)
	{
		Name = CardName.Festival;
		DefaultPrice = 5;
		AddActions = 2;
		AddBuys = 1;
		AddCoins = 2;
		festival = this;
	}

	public static Festival Get() => festival ?? new Festival();
}
