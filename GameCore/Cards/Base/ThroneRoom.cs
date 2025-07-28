using System.Linq;

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

	protected override void ActionEffect(IPlayer player)
	{
		var card = player.User.ThroneRoomPlay(this, player.PlayerState, player.Game.Kingdom, player.PlayerState.Hand.Where(c => c.IsAction));
		if (card == null)
		{
			return;
		}

		// TODO asi by bylo lepší tohle udělat přes player.PlayActionCard a přidat tam flag "without action" nebo něco takového
		player.PlayerState.Hand.Remove(card);
		player.PlayerState.PlayedCards.Add(card);
		for (int i = 0; i < 2; i++)
		{
			card.WhenPlayAction(player);
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
