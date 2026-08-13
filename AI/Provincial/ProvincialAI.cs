using AI.Model;
using GameCore;
using GameCore.Cards;

namespace AI.Provincial;
public class ProvincialAI : User
{
	PlayerInfo playerInfo = new();
	BuyAgenda buyAgenda;
	string name;

	public override string GetName() => name;

	public ProvincialAI(BuyAgenda buyAgenda, string name = nameof(ProvincialAI))
	{
		this.buyAgenda = buyAgenda.Clone();
		// todo vyřešit co se s agendou stane když je null (neměla by tu být vyjimka)
		this.name = name;
	}

	public override CardInstance PlayCard(IEnumerable<CardInstance> cards, PlayerState ps, Kingdom k, Phase phase, Card attackCard)
	{
		if (phase == Phase.Treasure)
		{
			return cards.FirstOrDefault(c => c.Card.IsTreasure);
		}

		float maxScore = 0;
		CardInstance bestCard = null;
		foreach (var c in cards)
		{
			var neco = Data.GetPriorityList()[(int)c.Card.Type];

			float score = c.Card.Score(cards.Select(c => c.Card), ps, phase);
			if (score >= maxScore)
			{
				maxScore = score;
				bestCard = c;
			}
		}

		return bestCard;
	}

	public override CardInstance SelectCardToGain(KingdomWrapper wrapper, PlayerState ps, Kingdom k, Phase phase)
	{
		var provinces = k.GetPile(CardType.Province);
		if (buyAgenda.Provinces > provinces.Count && wrapper.GetCard(CardType.Province) is not null)
		{
			return k.GetPile(CardType.Province).CardInstance;
		}

		if (buyAgenda.Duchies > provinces.Count && wrapper.GetCard(CardType.Duchy) is not null)
		{
			return k.GetPile(CardType.Duchy).CardInstance;
		}

		if (buyAgenda.Estates > provinces.Count && wrapper.GetCard(CardType.Estate) is not null)
		{
			return k.GetPile(CardType.Estate).CardInstance;
		}

		for (int i = 0; i < buyAgenda.BuyMenu.Count; i++)
		{
			var tuple = buyAgenda.BuyMenu[i];
			if (tuple.Number <= 0)
			{
				continue;
			}

			var cardInstance = wrapper.GetCard(tuple.Card);
			if (cardInstance is null)
			{
				continue;
			}

			tuple.Number--;
			if (tuple.Number == 0)
			{
				buyAgenda.BuyMenu.RemoveAt(i);
			}
			else
			{
				buyAgenda.BuyMenu[i] = tuple; // this is a value type, i have to return the value back
			}

			var card = cardInstance.Card;
			if (card.IsTreasure)
			{
				playerInfo.TreasureTotal += card.Coins;
			}

			switch (card.Type)
			{
				case CardType.Moneylender:
					playerInfo.TreasureTotal -= 1;
					break;
				case CardType.Bureaucrat:
					playerInfo.TreasureTotal += 2;
					break;
				case CardType.Mine:
					playerInfo.TreasureTotal += 1;
					break;
			}

			return cardInstance;
		}
		return null;
	}

	// the buy heuristic above already declines (returns null) when nothing on the agenda is
	// available, so it doubles as a reasonable "is this worth gaining" check for optional gains
	public override CardInstance SelectOptionalCardToGain(KingdomWrapper wrapper, PlayerState ps, Kingdom k, Phase phase)
		=> SelectCardToGain(wrapper, ps, k, phase);

	#region cards base
	public override CardInstance BureaucratPutOnTop(Card c, PlayerState ps, Kingdom k)
		=> ps.Hand.Where(c => c.IsVictory).First();

	public override List<CardInstance> CellarDiscard(Card c, PlayerState ps, Kingdom k)
		=> ps.Hand.Where(c => c.IsVictory && !c.IsTreasure && !c.IsAction).ToList();

	public override bool ChancellorDiscard(Card c, PlayerState ps, Kingdom k) => false;

	public override List<CardInstance> ChapelTrash(Card c, PlayerState ps, Kingdom k)
	{
		var cards = ps.Hand;

		// always trash curse
		var trash = cards.Where(c => c.Card.Type == CardType.Curse);

		var neco = trash.ToString();

		// in the beginning trash estate as well
		var provinces = k.GetPile(CardType.Province);
		if (buyAgenda.Estates <= provinces.Count)
		{
			trash = trash.Concat(cards.Where(c => c.Card.Type == CardType.Estate));
		}

		if (trash.Count() >= 4)
		{
			return trash.Take(4).ToList();
		}

		// trash only unnecesary coppers
		int coins = cards.Select(c => c.Card.Coins).Sum() + ps.Coins;
		var card = SelectCardToGain(k.GetWrapper(ps, coins), ps, k, Phase.Buy);
		int price = card is null ? 0 : card.Card.GetPrice(ps);

		if (playerInfo.TreasureTotal > 3)
		{
			var coppers = cards.Where(c => c.Card.Type == CardType.Copper).Take(coins - price);
			trash = trash.Concat(coppers);
			// player info update
			playerInfo.TreasureTotal -= coppers.Count();
		}

		return [.. trash.Take(4)];
	}

	public override bool LibrarySkip(Card c, PlayerState ps, Kingdom k, CardInstance card)
	{
		if (ps.Actions == 0)
		{
			return true;
		}

		if (ps.Actions == 1)
		{
			if (card.Card.AddActions > 0)
			{
				return false;
			}

			if (ps.Hand.Any(c => c.Card.AddActions > 1))
			{
				return false;
			}

			if (ps.Hand.Any(c => c.Card.AddActions == 0))
			{
				return true;
			}

			return false;
		}
		return false;
	}

	public override List<CardInstance> MilitiaDiscard(Card cardPlayed, PlayerState ps, Kingdom k, int discardCount)
	{
		var hand = ps.Hand.ToList();
		var discards = new List<CardInstance>();
		while (discards.Count < discardCount)
		{
			// first choice is random victory cardInstance
			var victoryCards = hand.Where(c => c.IsVictory);
			CardInstance card = victoryCards.Any() ? victoryCards.First() : null;

			// kdyz nemam victory tak vyberu nejzbytecnejsi kartu
			if (victoryCards.Count() == 0)
			{
				card = (from c in hand
						let m = hand.Min(a => a.Card.Score(hand.Select(card => card.Card), ps, Phase.Attack))
						where m == c.Card.Score(hand.Select(card => card.Card), ps, Phase.Attack)
						select c).FirstOrDefault();
			}
			discards.Add(card);
			hand.Remove(card);
		}

		return discards;
	}

	public override CardInstance MineTrash(Card card, PlayerState ps, Kingdom k, IList<CardInstance> cardSelection)
	{
		var coppers = ps.Hand.Where(a => a.Card.Type == CardType.Copper);
		if (coppers.Any())
		{
			return coppers.First();
		}
		var silvers = ps.Hand.Where(a => a.Card.Type == CardType.Silver);
		if (silvers.Any())
		{
			return silvers.First();
		}
		return null;
	}

	public override bool MoneylenderTrash(Card cardPlayed, PlayerState ps, Kingdom k) => true;

	public override CardInstance RemodelTrash(Card c, PlayerState ps, Kingdom k)
	{
		var trash = ps.Hand.Where(c => c.Card.Type == CardType.Curse);

		// at the end it will transform gold to province
		var provinces = k.GetPile(CardType.Province);
		if (buyAgenda.Provinces > provinces.Count && provinces.Count > 0)
		{
			trash = trash.Concat(ps.Hand.Where(c => c.Card.Type == CardType.Gold));
		}

		if (buyAgenda.Estates <= provinces.Count)
		{
			trash = trash.Concat(ps.Hand.Where(c => c.Card.Type == CardType.Estate));
		}

		trash = trash.Concat(ps.Hand.Where(c => c.Card.Type == CardType.Copper));
		trash = trash.Concat(ps.Hand.OrderBy(c => c.Card.Score(ps.Hand.Select(card => card.Card), ps, Phase.Action)));

		return trash.FirstOrDefault();
	}

	public override bool SpyDiscard(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c, Phase p)
	{
		if (p == Phase.Attack)
		{
			return !(c.IsVictory || c.Card.Type == CardType.Copper);
		}
		else // if (p == Phase.Action)
		{
			return (c.IsVictory || c.Card.Type == CardType.Copper);
		}
	}

	public override CardInstance ThiefChoose(Card c, PlayerState ps, Kingdom k, IEnumerable<CardInstance> cards)
		=> cards.OrderByDescending(c => c.Card.GetPrice(ps)).First();

	public override bool ThiefSteal(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c) => c.Card.GetPrice(ps) >= 3;

	public override CardInstance ThroneRoomPlay(Card cardPlayed, PlayerState ps, Kingdom k, IEnumerable<CardInstance> cards)
	{
		return (from c in cards
				let m = cards.Max(a => a.Card.Score(ps.Hand.Select(c => c.Card), ps, Phase.Action))
				where m == c.Card.Score(ps.Hand.Select(a => a.Card), ps, Phase.Action)
				select c).FirstOrDefault();
	}
	#endregion cards base

	#region cards intrique
	public override bool BaronDiscard(Card cardPlayed, PlayerState ps, Kingdom k) => true;

	public override CardInstance CourtyardPutOnTop(Card cardPlayed, PlayerState ps, Kingdom k, IEnumerable<CardInstance> cards)
	{
		throw new NotImplementedException();
	}

	public override CardInstance MasqueradePass(Card cardPlayed, PlayerState ps, Kingdom k, IEnumerable<CardInstance> cards)
	{
		throw new NotImplementedException();
	}

	public override CardInstance MasqueradeTrash(Card cardPlayed, PlayerState ps, Kingdom k, IEnumerable<CardInstance> cards)
	{
		throw new NotImplementedException();
	}

	public override bool MinionDiscard(Card cardPlayed, PlayerState ps, Kingdom k)
	{
		throw new NotImplementedException();
	}

	public override bool MiningVillageTrash(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c)
	{
		throw new NotImplementedException();
	}

	public override bool NoblesChooseCards(Card cardPlayed, PlayerState playerState, Kingdom kingdom)
	{
		throw new NotImplementedException();
	}

	public override bool TorturerChooseCurse(Card cardPlayed, PlayerState ps, Kingdom k)
	{
		throw new NotImplementedException();
	}

	public override List<CardInstance> TorturerDiscard(Card cardPlayed, PlayerState ps, Kingdom k, int discardCount)
	{
		throw new NotImplementedException();
	}

	public override List<CardInstance> TradingPostTrash(Card cardPlayed, PlayerState ps, Kingdom k)
	{
		throw new NotImplementedException();
	}

	#endregion cards intrique
}

