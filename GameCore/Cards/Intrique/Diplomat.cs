namespace GameCore.Cards.Intrique;
public class Diplomat : Card
{
	private static Diplomat diplomat;
	private Diplomat() : base
	(
		type: CardName.Diplomat,
		price: 4,
		addActions: 0,
		addBuys: 0,
		addCoins: 0,
		drawCards: 2,
		isVictory: false,
		isTreasure: false,
		isAction: true,
		isReaction: true,
		isAttack: false
	)
	{
		diplomat = this;
		Description = $"If you have 5 or fewer cards in hand (after drawing), +2 Actions. {Environment.NewLine}" +
			$"When another player plays an Attack card, you may first reveal this from a hand of 5 or more cards, to draw 2 cards then discard 3.";
	}

	public static Diplomat Get() => diplomat ?? new Diplomat();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		if (player.PlayerState.Hand.Count <= 5)
		{
			player.PlayerState.Actions += 2;
		}
	}

	public override bool Reaction(IPlayer player)
	{
		if (player.PlayerState.Hand.Count >= 5)
		{
			//TODO reveal itself
			player.Draw(2);
			List<CardInstance> cardsToDiscard = player.User.DiplomatDiscard(
				this, player.PlayerState, player.Game.Kingdom, player.PlayerState.Hand.ToList(), Math.Min(3, player.PlayerState.Hand.Count));
			player.PlayerState.DiscardPile.MoveRange(cardsToDiscard);
		}
		return base.Reaction(player);
	}
}
