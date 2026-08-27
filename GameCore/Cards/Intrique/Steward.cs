namespace GameCore.Cards.Intrique;

public class Steward : Card
{
	private static Steward steward;
	private Steward() : base
	(
		type: CardName.Steward,
		price: 3,
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
		steward = this;
		Description = "Choose one: +2 Cards; or +$2; or trash 2 cards from your hand.";
	}

	public static Steward Get() => steward ?? new Steward();

	private static readonly List<StewardBenefit> allBenefits = Enum.GetValues<StewardBenefit>().ToList();

	private static readonly Dictionary<StewardBenefit, Action<IPlayer, Card>> benefitEffects = new()
	{
		[StewardBenefit.Cards] = (p, s) => p.Draw(2),
		[StewardBenefit.Coins] = (p, s) => p.PlayerState.Coins += 2,
		[StewardBenefit.Trash] = (p, s) =>
		{
			List<CardInstance> cardsToTrash = p.User.StewardChooseCardsToTrash(
				s, p.PlayerState, p.Game.Kingdom, Math.Min(2, p.PlayerState.Hand.Count), p.PlayerState.Hand.ToList());
			cardsToTrash.ForEach(card => p.Trash(card));
		},
	};

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		StewardBenefit chosenBenefit = player.User.StewardChooseBenefit(this, player.PlayerState, player.Game.Kingdom, allBenefits);
		benefitEffects[chosenBenefit](player, this);
	}
}

public enum StewardBenefit
{
	Cards,
	Coins,
	Trash
}
