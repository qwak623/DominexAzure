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

	protected override void ActionEffect(Player player)
	{
		var card = player.User.ThroneRoomPlay(player.ps, player.Game.Kingdom, player.ps.Hand.Where(c => c.IsAction));
		if (card == null)
		{
			return;
		}

		player.ps.Hand.Remove(card);
		player.ps.PlayedCards.Add(card);
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
