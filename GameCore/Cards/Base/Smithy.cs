namespace GameCore.Cards.Base;
public class Smithy : Card
{
	private static Smithy smithy;
	private Smithy() : base(CardType.Action)
	{
		Name = CardName.Smithy;
		DefaultPrice = 4;
		DrawCards = 3;
		smithy = this;
	}

	public static Smithy Get() => smithy ?? new Smithy();
}
