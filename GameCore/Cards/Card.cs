namespace GameCore.Cards;

public abstract class Card
{

	protected readonly int price;

	public readonly CardName Name;
	public readonly int AddActions;
	public readonly int AddBuys;
	public readonly int AddCoins;
	public readonly int DrawCards;
	public readonly int Coins;

	public readonly bool IsVictory;
	public readonly bool IsTreasure;
	public readonly bool IsAction;
	public readonly bool IsReaction;
	public readonly bool IsAttack;

	public string Description { get; protected init; }

	// TODO je to trochu divné, asi by to chtělo dát jinam a rozdělit podle funkce
	public string Message { get; protected init; }

	public int VictoryPoints { get; protected init; }

	protected Card(CardName type, int price, int addActions, int addBuys, int addCoins, int drawCards, bool isVictory, bool isTreasure, bool isAction, bool isReaction, bool isAttack, string message = null)
	{
		Name = type;
		this.price = price;
		AddActions = addActions;
		AddBuys = addBuys;
		AddCoins = addCoins;
		DrawCards = drawCards;
		IsVictory = isVictory;
		IsTreasure = isTreasure;
		IsAction = isAction;
		IsReaction = isReaction;
		IsAttack = isAttack;
		Message = message;
	}

	protected Card(CardName type, int price, int addBuys, int coins, bool isVictory, bool isTreasure)
	{
		Name = type;
		this.price = price;
		AddBuys = addBuys;
		Coins = coins;
		IsVictory = isVictory;
		IsTreasure = isTreasure;
	}

	public virtual int GetPrice(PlayerState playerState)
	{
		var price = this.price - (playerState?.TempEffects?.GeneralCostReduction ?? 0);
		return price < 0 ? 0 : price;
	}
	public int DefaultPrice => price;

	/// <summary>
	/// Special action card effect including adding actions etc.
	/// </summary>
	/// <param name="player"></param>
	/// <param name="card"></param>
	public void WhenPlayAction(IPlayer player, CardInstance thisCard)
	{
		player.PlayerState.Actions += AddActions;
		player.PlayerState.Coins += AddCoins;
		player.PlayerState.Buys += AddBuys;
		if (DrawCards != 0)
		{
			player.Draw(DrawCards);
		}

		ActionEffect(player, thisCard);
	}

	protected void TriggerAttacks(IPlayer player)
	{
		foreach (var defender in player.Game.Players.Where(p => p != player))
		{
			defender.DealAttack(player, this);
		}
	}

	/// <summary>
	/// Template method with special card effect.
	/// Method is called in WhenPlayAction after adding actions, coins, buys and 
	/// drawing cards so theese effect shouldn be implemented here.
	/// </summary>
	/// <param name="player"></param>
	/// <param name="thisCard"></param>
	protected virtual void ActionEffect(IPlayer player, CardInstance thisCard) { }

	public void WhenPlayTreasure(IPlayer player)
	{
		// TODO proč tam jsou ty buye? 
		player.PlayerState.Buys += AddBuys;
		player.PlayerState.Coins += Coins;

		TreasureEffect(player);
	}

	protected virtual void TreasureEffect(IPlayer player) { }

	/// <summary>
	/// Returns true if attack was repulsed.
	/// </summary>
	/// <param name="game"></param>
	/// <param name="player"></param>
	/// <param name="user"></param>
	/// <returns></returns>
	public virtual bool Reaction(IPlayer player) => false;

	/// <summary>
	/// Returns number of victory points earned by a single copy of this card.
	/// Correct result appears only at the end of the game.
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	public virtual int CountPoints(IPlayer player) => VictoryPoints;

	/// <summary>
	/// Attack effect.
	/// </summary>
	/// <param name="defender"></param>
	/// <param name="attacker"></param>
	public virtual void Attack(IPlayer defender, IPlayer attacker) { }

	/// <summary>
	/// Some cards requires other cards, when they are in kingdom. (Witch requires Curse etc.)
	/// Update needed for some cards in extensions.
	/// </summary>
	public virtual Card RequiredCards => null;

	/// <summary>
	/// Returns instance of specified card type.
	/// </summary>
	/// <param name="cardType"></param>
	/// <returns></returns>
	public static Card Get(CardName cardType)
	{
		switch (cardType)
		{
			case CardName.NotDefined:
				return null;
			case CardName.Copper:
				return GeneralCards.Copper.Get();
			case CardName.Silver:
				return GeneralCards.Silver.Get();
			case CardName.Gold:
				return GeneralCards.Gold.Get();
			case CardName.Estate:
				return GeneralCards.Estate.Get();
			case CardName.Duchy:
				return GeneralCards.Duchy.Get();
			case CardName.Province:
				return GeneralCards.Province.Get();
			case CardName.Curse:
				return GeneralCards.Curse.Get();
			case CardName.Adventurer:
				return Base.Adventurer.Get();
			case CardName.Bureaucrat:
				return Base.Bureaucrat.Get();
			case CardName.Cellar:
				return Base.Cellar.Get();
			case CardName.CouncilRoom:
				return Base.CouncilRoom.Get();
			case CardName.Feast:
				return Base.Feast.Get();
			case CardName.Festival:
				return Base.Festival.Get();
			case CardName.Gardens:
				return Base.Gardens.Get();
			case CardName.Chancellor:
				return Base.Chancellor.Get();
			case CardName.Chapel:
				return Base.Chapel.Get();
			case CardName.Laboratory:
				return Base.Laboratory.Get();
			case CardName.Library:
				return Base.Library.Get();
			case CardName.Market:
				return Base.Market.Get();
			case CardName.Militia:
				return Base.Militia.Get();
			case CardName.Mine:
				return Base.Mine.Get();
			case CardName.Moat:
				return Base.Moat.Get();
			case CardName.Moneylender:
				return Base.Moneylender.Get();
			case CardName.Remodel:
				return Base.Remodel.Get();
			case CardName.Smithy:
				return Base.Smithy.Get();
			case CardName.Spy:
				return Base.Spy.Get();
			case CardName.Thief:
				return Base.Thief.Get();
			case CardName.ThroneRoom:
				return Base.ThroneRoom.Get();
			case CardName.Village:
				return Base.Village.Get();
			case CardName.Witch:
				return Base.Witch.Get();
			case CardName.Woodcutter:
				return Base.Woodcutter.Get();
			case CardName.Workshop:
				return Base.Workshop.Get();
			case CardName.Harbinger:
			case CardName.Merchant:
			case CardName.Vassal:
			case CardName.Poacher:
			case CardName.Bandit:
			case CardName.Sentry:
			case CardName.Artisan:
			case CardName.Courtyard:
				return Intrique.Courtyard.Get();
			case CardName.Pawn:
				return Intrique.Pawn.Get();
			case CardName.SecretChamber:
				return Intrique.SecretChamber.Get();
			case CardName.Masquerade:
				return Intrique.Masquerade.Get();
			case CardName.ShantyTown:
				return Intrique.ShantyTown.Get();
			case CardName.Steward:
				return Intrique.Steward.Get();
			case CardName.Swindler:
				return Intrique.Swindler.Get();
			case CardName.WishingWell:
				return Intrique.WishingWell.Get();
			case CardName.GreatHall:
				return Intrique.GreatHall.Get();
			case CardName.Harem:
				return Intrique.Harem.Get();
			case CardName.Baron:
				return Intrique.Baron.Get();
			case CardName.Bridge:
				return Intrique.Bridge.Get();
			case CardName.Conspirator:
				return Intrique.Conspirator.Get();
			case CardName.Ironworks:
				return Intrique.Ironworks.Get();
			case CardName.MiningVillage:
				return Intrique.MiningVillage.Get();
			case CardName.Coppersmith:
				return Intrique.Coppersmith.Get();
			case CardName.Scout:
				return Intrique.Scout.Get();
			case CardName.Duke:
				return Intrique.Duke.Get();
			case CardName.Minion:
				return Intrique.Minion.Get();
			case CardName.Torturer:
				return Intrique.Torturer.Get();
			case CardName.TradingPost:
				return Intrique.TradingPost.Get();
			case CardName.Upgrade:
				return Intrique.Upgrade.Get();
			case CardName.Saboteur:
				return Intrique.Saboteur.Get();
			case CardName.Tribute:
				return Intrique.Tribute.Get();
			case CardName.Nobles:
				return Intrique.Nobles.Get();
			case CardName.Lurker:
				return Intrique.Lurker.Get();
			case CardName.Diplomat:
				return Intrique.Diplomat.Get();
			case CardName.Mill:
				return Intrique.Mill.Get();
			case CardName.SecretPassage:
				return Intrique.SecretPassage.Get();
			case CardName.Courtier:
				return Intrique.Courtier.Get();
			case CardName.Patrol:
				return Intrique.Patrol.Get();
			case CardName.Replace:
				return Intrique.Replace.Get();
			default:
				throw new NotImplementedException();
		}
	}

	public override string ToString() => Name.ToDisplayName();
}