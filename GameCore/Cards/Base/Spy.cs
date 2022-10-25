using System.Linq;

namespace GameCore.Cards.Base;
public class Spy : Card
{
	private static Spy spy;
	private Spy() : base
	(
		name: "Spy",
		type: CardType.Spy,
		price: 4,
		addActions: 1,
		addBuys: 0,
		addCoins: 0,
		drawCards: 1,
		isVictory: false,
		isTreasure: false,
		isAction: true,
		isReaction: false,
		isAttack: true,
		message: "You may discard card at top of the draw pile."
	) => spy = this;

	public static Spy Get() => spy ?? new Spy();

	protected override void ActionEffect(Player player)
	{
		var card = player.Show(1).SingleOrDefault();
		if (card == null)
		{
			return;
		}

		if (player.User.SpyDiscard(player.ps, player.Game.Kingdom, card, Phase.Action))
		{
			player.Game.Logger?.Log($"{player.Name} discards {card.Name}");
			player.ps.DiscardPile.Add(card);
		}
		else
		{
			player.ps.DrawPile.Add(card);
		}
	}

	public override void Attack(Player defender, Player attacker)
	{
		var card = defender.Show(1).SingleOrDefault();
		if (card == null)
		{
			return;
		}

		if (attacker.User.SpyDiscard(attacker.ps, attacker.Game.Kingdom, card, Phase.Attack))
		{
			defender.Game.Logger?.Log($"{defender.Name} discards {card.Name}");
			defender.ps.DiscardPile.Add(card);
		}
		else
		{
			defender.ps.DrawPile.Add(card);
		}
	}
}
