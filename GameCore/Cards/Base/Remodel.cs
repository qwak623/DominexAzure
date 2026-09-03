namespace GameCore.Cards.Base;
public class Remodel : Card
{
	private static Remodel remodel;
	private Remodel() : base(CardType.Action)
	{
		Name = CardName.Remodel;
		DefaultPrice = 4;
		remodel = this;
		Description = "Trash a card from your hand. Gain a card costing up to $2 more than it.";
	}

	public static Remodel Get() => remodel ?? new Remodel();

	protected override void ActionEffect(IPlayer p, CardInstance thisCard)
	{
		// if user didn't select card, he wouldn't gain any.
		var oldCard = p.User.RemodelTrash(this, p.PlayerState, p.Game.Kingdom, p.PlayerState.Hand.ToList());
		if (oldCard is null)
		{
			return;
		}

		p.Trash(oldCard);

		var availableCards = p.Game.Kingdom.GetWrapper(p.PlayerState, oldCard.Card.GetPrice(p.PlayerState) + 2).AvailableCards.ToList();
		var newCard = p.User.SelectCardToGain(this, p.PlayerState, p.Game.Kingdom, availableCards);
		if (newCard is null)
		{
			return;
		}

		p.Gain(newCard.Card.Name);
	}
}
