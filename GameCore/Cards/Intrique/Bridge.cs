namespace GameCore.Cards.Intrique;

public class Bridge : Card
{
	private static Bridge bridge;
	private Bridge() : base(CardType.Action)
	{
		Name = CardName.Bridge;
		DefaultPrice = 4;
		AddBuys = 1;
		AddCoins = 1;
		bridge = this;
		Description = "This turn, cards (everywhere) cost $1 less.";
	}

	public static Bridge Get() => bridge ?? new Bridge();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		player.PlayerState.TempEffects.ReduceCost(1);
	}
}
