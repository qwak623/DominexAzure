namespace GameCore.Cards.GeneralCards;

public class Duchy : Card
{
	private static Duchy duchy;
	private Duchy() : base(CardType.Victory)
	{
		Name = CardName.Duchy;
		DefaultPrice = 5;
		duchy = this;
		VictoryPoints = 3;
	}

	public static Duchy Get() => duchy ?? new Duchy();
}
