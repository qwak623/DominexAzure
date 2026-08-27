namespace GameCore.Cards.Intrique;

public class Conspirator : Card
{
	private static Conspirator conspirator;
	private Conspirator() : base(CardType.Action)
	{
		Name = CardName.Conspirator;
		DefaultPrice = 4;
		AddCoins = 2;
		conspirator = this;
		Description = "If you've played 3 or more Actions this turn (counting this), +1 Card and +1 Action.";
	}

	public static Conspirator Get() => conspirator ?? new Conspirator();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		if (player.PlayerState.ActionsPlayed.Count >= 3)
		{
			player.Draw(1);
			player.PlayerState.Actions += 1;
		}
	}
}
