namespace GameCore.Cards;

public abstract class Card
{

	protected readonly int price;

	public readonly string Name;
	public readonly CardType Type;
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

	protected Card(string name, CardType type, int price, int addActions, int addBuys, int addCoins, int drawCards, bool isVictory, bool isTreasure, bool isAction, bool isReaction, bool isAttack, string message = null)
	{
		Name = name;
		Type = type;
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

	protected Card(string name, CardType type, int price, int addBuys, int coins, bool isVictory, bool isTreasure)
	{
		Name = name;
		Type = type;
		this.price = price;
		AddBuys = addBuys;
		Coins = coins;
		IsVictory = isVictory;
		IsTreasure = isTreasure;
	}

	public virtual int GetPrice(PlayerState playerState)
	{
		var price = this.price - playerState.TempEffects.GeneralCostReduction;
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
	public static Card Get(CardType cardType)
	{
		switch (cardType)
		{
			case CardType.NotDefined:
				return null;
			case CardType.Copper:
				return GeneralCards.Copper.Get();
			case CardType.Silver:
				return GeneralCards.Silver.Get();
			case CardType.Gold:
				return GeneralCards.Gold.Get();
			case CardType.Estate:
				return GeneralCards.Estate.Get();
			case CardType.Duchy:
				return GeneralCards.Duchy.Get();
			case CardType.Province:
				return GeneralCards.Province.Get();
			case CardType.Curse:
				return GeneralCards.Curse.Get();
			case CardType.Adventurer:
				return Base.Adventurer.Get();
			case CardType.Bureaucrat:
				return Base.Bureaucrat.Get();
			case CardType.Cellar:
				return Base.Cellar.Get();
			case CardType.CouncilRoom:
				return Base.CouncilRoom.Get();
			case CardType.Feast:
				return Base.Feast.Get();
			case CardType.Festival:
				return Base.Festival.Get();
			case CardType.Gardens:
				return Base.Gardens.Get();
			case CardType.Chancellor:
				return Base.Chancellor.Get();
			case CardType.Chapel:
				return Base.Chapel.Get();
			case CardType.Laboratory:
				return Base.Laboratory.Get();
			case CardType.Library:
				return Base.Library.Get();
			case CardType.Market:
				return Base.Market.Get();
			case CardType.Militia:
				return Base.Militia.Get();
			case CardType.Mine:
				return Base.Mine.Get();
			case CardType.Moat:
				return Base.Moat.Get();
			case CardType.Moneylender:
				return Base.Moneylender.Get();
			case CardType.Remodel:
				return Base.Remodel.Get();
			case CardType.Smithy:
				return Base.Smithy.Get();
			case CardType.Spy:
				return Base.Spy.Get();
			case CardType.Thief:
				return Base.Thief.Get();
			case CardType.ThroneRoom:
				return Base.ThroneRoom.Get();
			case CardType.Village:
				return Base.Village.Get();
			case CardType.Witch:
				return Base.Witch.Get();
			case CardType.Woodcutter:
				return Base.Woodcutter.Get();
			case CardType.Workshop:
				return Base.Workshop.Get();
			case CardType.Harbinger:
			case CardType.Merchant:
			case CardType.Vassal:
			case CardType.Poacher:
			case CardType.Bandit:
			case CardType.Sentry:
			case CardType.Artisan:
			case CardType.Courtyard:
			case CardType.Pawn:
			case CardType.SecretChamber:
			case CardType.Masquerade:
			case CardType.ShantyTown:
			case CardType.Steward:
			case CardType.Swindler:
			case CardType.WishingWell:
			case CardType.GreatHall:
			case CardType.Baron:
			case CardType.Bridge:
			case CardType.Conspirator:
			case CardType.Ironworks:
			case CardType.MiningVillage:
			case CardType.Coppersmith:
			case CardType.Scout:
			case CardType.Duke:
			case CardType.Minion:
			case CardType.Torturer:
			case CardType.TradingPost:
			case CardType.Upgrade:
			case CardType.Saboteur:
			case CardType.Tribute:
			case CardType.Nobbles:
			case CardType.Lurker:
			case CardType.Diplomat:
			case CardType.Mill:
			case CardType.SecretPassage:
			case CardType.Courtier:
			case CardType.Patrol:
			case CardType.Replace:
			default:
				throw new NotImplementedException();
		}
	}

	public override string ToString() => Name;
}