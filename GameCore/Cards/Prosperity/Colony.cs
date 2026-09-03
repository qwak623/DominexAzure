namespace GameCore.Cards.Prosperity;

public class Colony : Card
{
	private static Colony colony;
	private Colony() : base(CardType.Victory)
	{
		Name = CardName.Colony;
		DefaultPrice = 11;
		colony = this;
		VictoryPoints = 10;
	}

	public override int GetCountInKingdomPile(int playerCount)
	{
		if (playerCount < 2 || playerCount > 6)
		{
			throw new InvalidOperationException();
		}
		if (playerCount == 2)
		{
			return 8;
		}
		return 12;
	}

	public static Colony Get() => colony ?? new Colony();
}
