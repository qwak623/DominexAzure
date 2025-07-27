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
		isAttack: true
	)
	{
		bureaucrat = this;
		Description = "Gain a Silver onto your deck. Each other player reveals a Victory card from their hand and puts it onto their deck (or reveals a hand with no Victory cards).";
		Message = "Return card with victory points up to draw pile, if you have any.";
	}

	public static Bureaucrat Get() => bureaucrat ?? new Bureaucrat();

	protected override void ActionEffect(IPlayer player) => player.GainToDrawPile(CardType.Silver);

	public override void Attack(IPlayer def, IPlayer att)
	{
		// TODO REVEAL hand with no victory cards
		if (!def.PlayerState.Hand.Any(c => c.IsVictory))
		{
			return;
		}

		var card = def.User.BureaucratPutOnTop(this, def.PlayerState, def.Game.Kingdom);
		def.ReturnToDrawPile(card);
	}
}