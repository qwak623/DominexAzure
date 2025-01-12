using GameCore;
using GameCore.Cards;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AI.Provincial
{
    static class PlayAgenda
    {
        public static float Score(this Card card, IEnumerable<Card> hand, PlayerState ps, Phase phase)
        {
            float result = 0;
            if (phase == Phase.Action && card.AddActions >= 1 && ps.Actions == 1)
                result += 100;
            switch (phase)
            {
                case Phase.Action:
                    if (card.Type == CardType.Chapel)
                        return result + hand.Where(c => c.Type == CardType.Curse).Count() * 3 + Data.GetPriorityList()[(int)card.Type];
                    if (card.Type == CardType.Library)
                        return result + -1.5f + 3 * (7 - ps.Hand.Count);
                    if (card.Type == CardType.Remodel)
                        return result + (hand.Any(c => c.Type == CardType.Curse) ? 1 : 0) * 3 + Data.GetPriorityList()[(int)card.Type];
                    if (card.Type == CardType.Moneylender && ps.Hand.Contains(CardType.Copper))
                        return -1;
                    if (card.Type == CardType.Mine && !(ps.Hand.Contains(CardType.Copper) || ps.Hand.Contains(CardType.Silver)))
                        return -1;  
                    return Data.GetPriorityList()[(int)card.Type];
                case Phase.Attack:
                    if (card.Type == CardType.Gold)
                        return 100;
                    if (card.Type == CardType.Silver)
                        return 50;
                    if (card.Type == CardType.Library)
                        return 49;
                    if (card.Type == CardType.Copper)
                        return 15;
                    goto case Phase.Action;
                default:
                    break;
            }

            return -1;
        }
    }
}