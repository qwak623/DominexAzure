namespace GameCore.Cards.Base;
public class ThroneRoom : Card
{
	private static ThroneRoom throneRoom;
	private ThroneRoom() : base
	(
		name: "Throne Room",
		type: CardType.ThroneRoom,
		price: 4,
		addActions: 0,
		addBuys: 0,
		addCoins: 0,
		drawCards: 0,
		isVictory: false,
		isTreasure: false,
		isAction: true,
		isReaction: false,
		isAttack: false
	)
	{
		throneRoom = this;
		Description = "You may play an Action card from your hand twice.";
		Message = "You may play an Action card from your hand twice.";
	}

	public static ThroneRoom Get() => throneRoom ?? new ThroneRoom();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		var cardInstance = player.User
			.ThroneRoomPlay(this, player.PlayerState, player.Game.Kingdom, player.PlayerState.Hand.Where(c => c.IsAction));
		if (cardInstance is null)
		{
			return;
		}

		player.PlayerState.CardsPlayed.Move(cardInstance);
		var card = cardInstance.Card;
		for (int i = 0; i < 2; i++)
		{
			player.PlayerState.ActionsPlayed.Add(card);
			cardInstance.WhenPlayAction(player);
			if (card.IsAttack)
			{
				foreach (var defender in player.Game.Players.Where(p => p != player))
				{
					defender.DealAttack(player, card);
				}
			}
		}
	}
}
