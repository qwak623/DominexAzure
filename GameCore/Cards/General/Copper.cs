namespace GameCore.Cards.GeneralCards;
public class Copper : Card
{
	private static Copper copper;
	private Copper() : base(CardType.Treasure)
	{
		Name = CardName.Copper;
		DefaultPrice = 0;
		Coins = 1;
		copper = this;
	}

	public override int GetCountInKingdomPile(int playerCount) => 60;

	public static Copper Get() => copper ?? new Copper();

	protected override void TreasureEffect(IPlayer player)
	{
		player.PlayerState.Coins += player.PlayerState.TempEffects.CopperValueIncrease;
	}
}
