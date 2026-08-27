namespace GameCore.Cards.Base;
public class Bureaucrat : Card
{
	private static Bureaucrat bureaucrat;

	private Bureaucrat() : base([CardType.Action, CardType.Attack])
	{
		Name = CardName.Bureaucrat;
		DefaultPrice = 4;
		bureaucrat = this;
		Description = "Gain a Silver onto your deck. " +
			"Each other player reveals a Victory card from their hand and puts it onto their deck (or reveals a hand with no Victory cards).";
		Message = "Return card with victory points up to draw pile, if you have any.";
	}

	public static Bureaucrat Get() => bureaucrat ?? new Bureaucrat();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		player.GainToDrawPile(CardName.Silver);
		TriggerAttacks(player);
	}

	public override void Attack(IPlayer def, IPlayer att)
	{
		// TODO REVEAL hand with no victory cards
		var victoryCards = def.PlayerState.Hand.Where(c => c.IsVictory).ToList();
		if (victoryCards.Count == 0)
		{
			return;
		}

		var card = def.User.BureaucratPutOnTop(this, def.PlayerState, def.Game.Kingdom, victoryCards);
		def.ReturnToDrawPile(card);
	}
}