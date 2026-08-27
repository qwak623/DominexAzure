namespace GameCore.Cards.Base;
public class Mine : Card
{
	private static Mine mine;
	private Mine() : base(CardType.Action)
	{
		Name = CardName.Mine;
		DefaultPrice = 5;
		mine = this;
		Description = "You may trash a Treasure card from your hand. Gain a Treasure card to your hand costing up to $3 more than it.";
		Message = "Trash a treasure, gain a treasure to your hand costing up to $3 more.";
	}

	public static Mine Get() => mine ?? new Mine(); // todo tohle neni thread safe - je potreba aby bylo?

	protected override void ActionEffect(IPlayer p, CardInstance thisCard)
	{
		var cardSelection = p.PlayerState.Hand.Where(c => c.IsTreasure).ToList();

		var oldCard = p.User.MineTrash(this, p.PlayerState, p.Game.Kingdom, cardSelection);
		if (oldCard is null)
		{
			return;
		}

		p.Trash(oldCard);
		var newCard = p.User.SelectCardToGain(
			p.Game.Kingdom.GetWrapper(p.PlayerState, oldCard.Card.GetPrice(p.PlayerState) + 3, true), p.PlayerState, p.Game.Kingdom, Phase.Gain);
		if (newCard is not null)
		{
			p.GainToHand(newCard);
		}
	}
}
