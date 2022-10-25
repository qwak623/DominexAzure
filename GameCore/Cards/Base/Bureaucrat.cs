using System.Linq;

namespace GameCore.Cards.Base;
public class Bureaucrat : Card
{
	private static Bureaucrat bureaucrat;

	private Bureaucrat() : base
	(
		name: "Bureaucrat",
		type: CardType.Bureaucrat,
		price: 4,
		addActions: 0,
		addBuys: 0,
		addCoins: 0,
		drawCards: 0,
		isVictory: false,
		isTreasure: false,
		isAction: true,
		isReaction: false,
		isAttack: true,
		message: "Return card with victory points up to draw pile, if you have any."
	) => bureaucrat = this;

	public static Bureaucrat Get() => bureaucrat ?? new Bureaucrat();

	protected override void ActionEffect(Player player) => player.GainToDrawPile(CardType.Silver);

	public override void Attack(Player def, Player att)
	{
		if (!def.ps.Hand.Any(c => c.IsVictory))
			return;
		var card = def.User.BureaucratDiscard(def.ps, def.Game.Kingdom);
		def.ReturnToDrawPile(card);
	}
}