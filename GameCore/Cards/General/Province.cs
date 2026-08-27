namespace GameCore.Cards.GeneralCards;
public class Province : Card
{
	private static Province province;
	private Province() : base(CardType.Victory)
	{
		Name = CardName.Province;
		DefaultPrice = 8;
		province = this;
		VictoryPoints = 6;
	}

	public override int GetCountInKingdomPile(int playerCount)
	{
		return playerCount switch
		{
			2 => 8,
			3 or 4 => 12,
			5 => 15,
			6 => 18,
			_ => throw new InvalidOperationException()
		};
	}

	public static Province Get() => province ?? new Province();
}
