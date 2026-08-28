namespace GameCore.Cards.GeneralCards;
public class Silver : Card
{
	private static Silver silver;
	private Silver() : base(CardType.Treasure)
	{
		Name = CardName.Silver;
		DefaultPrice = 3;
		Coins = 2;
		silver = this;
	}

	public override int GetCountInKingdomPile(int playerCount) => 40;

	protected override void TreasureEffect(IPlayer player)
	{
		player.PlayerState.Coins += player.PlayerState.TempEffects.FirstSilverValueIncrease;
		player.PlayerState.TempEffects.ClearFirstSilverValue();
	}

	public static Silver Get() => silver ?? new Silver();
}
