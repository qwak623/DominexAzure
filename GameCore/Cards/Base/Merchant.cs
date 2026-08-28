namespace GameCore.Cards.Base;

public class Merchant : Card
{
	private static Merchant merchant;
	private Merchant() : base(CardType.Action)
	{
		Name = CardName.Merchant;
		DefaultPrice = 3;
		AddActions = 1;
		DrawCards = 1;
		merchant = this;
		Description = "The first time you play a Silver this turn, +$1.";
	}

	public static Merchant Get() => merchant ?? new Merchant();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		player.PlayerState.TempEffects.IncreaseFirstSilverValue();
	}
}
