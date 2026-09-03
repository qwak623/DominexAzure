namespace GameCore.Cards.Intrique;
public class Replace : Card
{
	private static Replace remodel;
	private Replace() : base([CardType.Action, CardType.Attack])
	{
		Name = CardName.Replace;
		DefaultPrice = 5;
		remodel = this;
		Description = "Trash a card from your hand. Gain a card costing up to $2 more than it. " +
			"If the gained card is an Action or Treasure, put it onto your deck; if it's a Victory card, each other player gains a Curse.";
	}

	public static Replace Get() => remodel ?? new Replace();

	public override Card RequiredCards => GeneralCards.Curse.Get();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		var oldCard = player.User.ReplaceTrash(this, player.PlayerState, player.Game.Kingdom, player.PlayerState.Hand.ToList());
		if (oldCard is null)
		{
			return;
		}
		player.Trash(oldCard);

		var availableCards = player.Game.Kingdom.GetWrapper(player.PlayerState, oldCard.Card.GetPrice(player.PlayerState) + 2).AvailableCards.ToList();
		var newCard = player.User.SelectCardToGain(this, player.PlayerState, player.Game.Kingdom, availableCards);
		if (newCard is null)
		{
			return;
		}

		player.Gain(newCard);
		if (newCard.IsAction || newCard.IsTreasure)
		{
			player.ReturnToDrawPile(newCard);
		}
		if (newCard.IsVictory)
		{
			TriggerAttacks(player);
		}
	}

	public override void Attack(IPlayer defender, IPlayer attacker) => defender.Gain(CardName.Curse);
}
