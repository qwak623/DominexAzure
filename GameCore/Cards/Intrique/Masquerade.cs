namespace GameCore.Cards.Intrique;
public class Masquerade : Card
{
	private static Masquerade masquerade;
	private Masquerade() : base
	(
		type: CardName.Masquerade,
		price: 3,
		addActions: 0,
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
		masquerade = this;
		Description = "Each player with any cards in hand passes one to the next such player to the left, at once. " +
			"Then you may trash a card from your hand.";
	}

	public static Masquerade Get() => masquerade ?? new Masquerade();
	protected override void ActionEffect(IPlayer p, CardInstance thisCard)
	{

		var playersWithCardsInHand = p.Game.Players.Where(player => player.PlayerState.Hand.Count > 0).ToList();
		if (playersWithCardsInHand.Count > 1)
		{
			var cardsToPass = new Dictionary<IPlayer, CardInstance>();
			foreach (var player in playersWithCardsInHand)
			{
				cardsToPass[player] = player.User.MasqueradePass(
					this, player.PlayerState, p.Game.Kingdom, player.PlayerState.Hand.ToList());
			}
			for (int i = 0; i < playersWithCardsInHand.Count; i++)
			{
				playersWithCardsInHand[(i + 1) % playersWithCardsInHand.Count].PlayerState.Hand
					.Move(cardsToPass[playersWithCardsInHand[i]]);
			}
		}
		var cardToTrash = p.User.MasqueradeTrash(this, p.PlayerState, p.Game.Kingdom, p.PlayerState.Hand.ToList());
		if (cardToTrash is not null)
		{
			p.Trash(cardToTrash);
		}
	}
}
