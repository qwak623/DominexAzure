namespace GameCore.Cards.Base;
public class Laboratory : Card
{
	private static Laboratory laboratory;
	private Laboratory() : base(CardType.Action)
	{
		Name = CardName.Laboratory;
		DefaultPrice = 5;
		AddActions = 1;
		DrawCards = 2;
		laboratory = this;
	}

	public static Laboratory Get() => laboratory ?? new Laboratory();
}

