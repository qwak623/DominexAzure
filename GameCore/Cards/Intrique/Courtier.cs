using GameCore.GameCore;

namespace GameCore.Cards.Intrique;

public class Courtier : Card
{
	private static Courtier courtier;
	private Courtier() : base(CardType.Action)
	{
		Name = CardName.Courtier;
		DefaultPrice = 5;
		courtier = this;
		Description = "Reveal a card from your hand. For each type it has (Action, Attack, etc.), " +
			"choose one: +1 Action; or +1 Buy; or +$3; or gain a Gold. The choices must be different.";
	}

	public static Courtier Get() => courtier ?? new Courtier();

	private static readonly List<CourtierBenefit> allBenefits = Enum.GetValues<CourtierBenefit>().ToList();

	private static readonly Dictionary<CourtierBenefit, Action<IPlayer>> benefitEffects = new()
	{
		[CourtierBenefit.Action] = p => p.PlayerState.Actions++,
		[CourtierBenefit.Buy] = p => p.PlayerState.Buys++,
		[CourtierBenefit.Coins] = p => p.PlayerState.Coins += 3,
		[CourtierBenefit.GainGold] = p => p.Gain(CardName.Gold),
	};

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		if (player.PlayerState.Hand.Count == 0)
		{
			player.Game.Logger?.Log(new GameLog { PlayerId = player.Name, Message = $"{player.Name} has no cards in hand to reveal." });
			return;
		}

		CardInstance revealedCard = player.User.CourtierReveal(this, player.PlayerState, player.Game.Kingdom, player.PlayerState.Hand.ToList());

		int benefitCount = Math.Min(revealedCard.Card.CardTypes.Count, allBenefits.Count);
		if (benefitCount == 0)
		{
			return;
		}

		List<CourtierBenefit> chosenBenefits = player.User.CourtierChooseBenefits(
			this, player.PlayerState, player.Game.Kingdom, benefitCount, allBenefits);
		chosenBenefits.ForEach(benefit => benefitEffects[benefit](player));
	}
}

public enum CourtierBenefit
{
	Action,
	Buy,
	Coins,
	GainGold
}
