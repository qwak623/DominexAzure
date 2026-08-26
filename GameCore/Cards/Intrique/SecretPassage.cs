namespace GameCore.Cards.Intrique;

public class SecretPassage : Card
{
	private static SecretPassage secretPassage;
	private SecretPassage() : base
	(
		name: "Secret Passage",
		type: CardType.SecretPassage,
		price: 4,
		addActions: 1,
		addBuys: 0,
		addCoins: 0,
		drawCards: 2,
		isVictory: false,
		isTreasure: false,
		isAction: true,
		isReaction: false,
		isAttack: false
	)
	{
		secretPassage = this;
		Description = $"Take a card from your hand and put it anywhere in your deck.";
	}

	public static SecretPassage Get() => secretPassage ?? new SecretPassage();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		if (player.PlayerState.Hand.Count == 0)
		{
			return;
		}
		CardInstance card = player.User.SecretPassageChooseCard(this, player.PlayerState, player.Game.Kingdom, player.PlayerState.Hand.ToList());
		// TODO put anywhere in the deck.
		player.ReturnToDrawPile(card);
	}
}
