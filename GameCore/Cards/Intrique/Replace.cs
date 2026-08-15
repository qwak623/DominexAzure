namespace GameCore.Cards.Intrique;
public class Replace : Card
{
	private static Replace remodel;
	private Replace() : base
	(
		name: "Replace",
		type: CardType.Replace,
		price: 5,
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
		remodel = this;
		Description = "Trash a card from your hand. Gain a card costing up to $2 more than it. " +
			"If the gained card is an Action or Treasure, put it onto your deck; if it's a Victory card, each other player gains a Curse.";
	}

	public static Replace Get() => remodel ?? new Replace();

	public override Card RequiredCards => GeneralCards.Curse.Get();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		if (player.PlayerState.Hand.Count == 0)
		{
			return;
		}
		var oldCard = player.User.ReplaceTrash(this, player.PlayerState, player.Game.Kingdom, player.PlayerState.Hand.ToList());
		player.Trash(oldCard);

		var newCard = player.User.SelectCardToGain(
			player.Game.Kingdom.GetWrapper(player.PlayerState, oldCard.Card.GetPrice(player.PlayerState) + 2),
			player.PlayerState, player.Game.Kingdom, Phase.Gain);
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

	public override void Attack(IPlayer defender, IPlayer attacker) => defender.Gain(CardType.Curse);
}
