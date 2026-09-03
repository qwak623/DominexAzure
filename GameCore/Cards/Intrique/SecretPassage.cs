namespace GameCore.Cards.Intrique;

public class SecretPassage : Card
{
	private static SecretPassage secretPassage;
	private SecretPassage() : base(CardType.Action)
	{
		Name = CardName.SecretPassage;
		DefaultPrice = 4;
		AddActions = 1;
		DrawCards = 2;
		secretPassage = this;
		Description = $"Take a card from your hand and put it anywhere in your deck.";
	}

	public static SecretPassage Get() => secretPassage ?? new SecretPassage();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		CardInstance card = player.User.SecretPassageChooseCard(
			this, player.PlayerState, player.Game.Kingdom, player.PlayerState.Hand.ToList());
		// TODO put anywhere in the deck.
		player.ReturnToDrawPile(card);
	}
}
