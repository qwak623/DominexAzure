namespace GameCore.Cards.GeneralCards;

public class Estate : Card
{
	private static Estate estate;
	private Estate() : base(CardType.Victory)
	{
		Name = CardName.Estate;
		DefaultPrice = 2;
		estate = this;
		VictoryPoints = 1;
	}

	public static Estate Get() => estate ?? new Estate();
}
