namespace GameCore.Cards;

public abstract class Card
{
	public CardName Name { get; protected init; }
	public int DefaultPrice { get; protected init; }
	public int AddActions { get; protected init; }
	public int AddBuys { get; protected init; }
	public int AddCoins { get; protected init; }
	public int DrawCards { get; protected init; }
	public int Coins { get; protected init; }
	public int VictoryPoints { get; protected init; }


	public List<CardType> CardTypes { get; protected init; }
	public bool IsVictory => CardTypes.Contains(CardType.Victory);
	public bool IsTreasure => CardTypes.Contains(CardType.Treasure);
	public bool IsAction => CardTypes.Contains(CardType.Action);
	public bool IsReaction => CardTypes.Contains(CardType.Reaction);
	public bool IsAttack => CardTypes.Contains(CardType.Attack);

	public string Description { get; protected init; }

	// TODO je to trochu divné, asi by to chtělo dát jinam a rozdělit podle funkce
	public string Message { get; protected init; }


	protected Card(CardType type)
	{
		CardTypes = [type];
	}

	protected Card(List<CardType> types)
	{
		CardTypes = types;
	}

	public virtual int GetPrice(PlayerState playerState)
	{
		var price = DefaultPrice - (playerState?.TempEffects?.GeneralCostReduction ?? 0);
		return price < 0 ? 0 : price;
	}

	public virtual int GetCountInKingdomPile(int playerCount)
	{
		if (IsVictory)
		{
			return (playerCount == 2) ? 8 : 12;
		}
		return 10;
	}

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
	/// Returns instance of specified card name.
	/// </summary>
	/// <param name="cardType"></param>
	/// <returns></returns>
	public static Card Get(CardName cardType)
	{
		return cardType switch
		{
			CardName.NotDefined => null,
			CardName.Copper => GeneralCards.Copper.Get(),
			CardName.Silver => GeneralCards.Silver.Get(),
			CardName.Gold => GeneralCards.Gold.Get(),
			CardName.Estate => GeneralCards.Estate.Get(),
			CardName.Duchy => GeneralCards.Duchy.Get(),
			CardName.Province => GeneralCards.Province.Get(),
			CardName.Curse => GeneralCards.Curse.Get(),
			CardName.Adventurer => Base.Adventurer.Get(),
			CardName.Bureaucrat => Base.Bureaucrat.Get(),
			CardName.Cellar => Base.Cellar.Get(),
			CardName.CouncilRoom => Base.CouncilRoom.Get(),
			CardName.Feast => Base.Feast.Get(),
			CardName.Festival => Base.Festival.Get(),
			CardName.Gardens => Base.Gardens.Get(),
			CardName.Chancellor => Base.Chancellor.Get(),
			CardName.Chapel => Base.Chapel.Get(),
			CardName.Laboratory => Base.Laboratory.Get(),
			CardName.Library => Base.Library.Get(),
			CardName.Market => Base.Market.Get(),
			CardName.Militia => Base.Militia.Get(),
			CardName.Mine => Base.Mine.Get(),
			CardName.Moat => Base.Moat.Get(),
			CardName.Moneylender => Base.Moneylender.Get(),
			CardName.Remodel => Base.Remodel.Get(),
			CardName.Smithy => Base.Smithy.Get(),
			CardName.Spy => Base.Spy.Get(),
			CardName.Thief => Base.Thief.Get(),
			CardName.ThroneRoom => Base.ThroneRoom.Get(),
			CardName.Village => Base.Village.Get(),
			CardName.Witch => Base.Witch.Get(),
			CardName.Woodcutter => Base.Woodcutter.Get(),
			CardName.Workshop => Base.Workshop.Get(),
			CardName.Bandit => Base.Bandit.Get(),
			CardName.Artisan => Base.Artisan.Get(),
			CardName.Harbinger => Base.Harbinger.Get(),
			CardName.Merchant => Base.Merchant.Get(),
			CardName.Poacher => Base.Poacher.Get(),
			CardName.Vassal => Base.Vassal.Get(),
			CardName.Courtyard => Intrique.Courtyard.Get(),
			CardName.Pawn => Intrique.Pawn.Get(),
			CardName.Sentry => Base.Sentry.Get(),
			CardName.SecretChamber => Intrique.SecretChamber.Get(),
			CardName.Masquerade => Intrique.Masquerade.Get(),
			CardName.ShantyTown => Intrique.ShantyTown.Get(),
			CardName.Steward => Intrique.Steward.Get(),
			CardName.Swindler => Intrique.Swindler.Get(),
			CardName.WishingWell => Intrique.WishingWell.Get(),
			CardName.GreatHall => Intrique.GreatHall.Get(),
			CardName.Harem => Intrique.Harem.Get(),
			CardName.Baron => Intrique.Baron.Get(),
			CardName.Bridge => Intrique.Bridge.Get(),
			CardName.Conspirator => Intrique.Conspirator.Get(),
			CardName.Ironworks => Intrique.Ironworks.Get(),
			CardName.MiningVillage => Intrique.MiningVillage.Get(),
			CardName.Coppersmith => Intrique.Coppersmith.Get(),
			CardName.Scout => Intrique.Scout.Get(),
			CardName.Duke => Intrique.Duke.Get(),
			CardName.Minion => Intrique.Minion.Get(),
			CardName.Torturer => Intrique.Torturer.Get(),
			CardName.TradingPost => Intrique.TradingPost.Get(),
			CardName.Upgrade => Intrique.Upgrade.Get(),
			CardName.Saboteur => Intrique.Saboteur.Get(),
			CardName.Tribute => Intrique.Tribute.Get(),
			CardName.Nobles => Intrique.Nobles.Get(),
			CardName.Lurker => Intrique.Lurker.Get(),
			CardName.Diplomat => Intrique.Diplomat.Get(),
			CardName.Mill => Intrique.Mill.Get(),
			CardName.SecretPassage => Intrique.SecretPassage.Get(),
			CardName.Courtier => Intrique.Courtier.Get(),
			CardName.Patrol => Intrique.Patrol.Get(),
			CardName.Replace => Intrique.Replace.Get(),
			_ => throw new NotImplementedException(),
		};
	}

	public override string ToString() => Name.ToDisplayName();
}