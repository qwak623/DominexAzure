using AI.Model;
using GameCore.Cards;
using System.Collections.Generic;
using Utils;

namespace AI.Evolution
{
    // this class is not thread safe
    abstract class Mutation
    {
        protected ThreadSafeRandom rnd = new ThreadSafeRandom();

        /// <summary>
        /// Mutation changes agenda in parameter based on kingdom kards and it does not create new buyAgenda.
        /// </summary>
        /// <param name="agenda"></param>
        /// <param name="kingdom"></param>
        public abstract void Mutate(BuyAgenda agenda, List<Card> kingdom);
    }

    class ReplaceSupplyCardMutation : Mutation
    {
        public override void Mutate(BuyAgenda agenda, List<Card> kingdom)
        {
            if (agenda.BuyMenu.Count == 0)
                return;

            int i = rnd.Next(agenda.BuyMenu.Count);
            int j = rnd.Next(kingdom.Count); 

            var tuple = agenda.BuyMenu[i];
            tuple.Card = kingdom[j].Type;
            agenda.BuyMenu[i] = tuple;
        }
    }

    class ModifyPurchaseCountMutation : Mutation
    {
        public override void Mutate(BuyAgenda agenda, List<Card> kingdom)
        {
            if (agenda.BuyMenu.Count == 0)
                return;

            int i = rnd.Next(agenda.BuyMenu.Count);

            var tuple = agenda.BuyMenu[i];

            tuple.Number += rnd.NextSign();

            // if number = 0 card is never bought anyway
            if (tuple.Number == 0)
            {
                agenda.BuyMenu.RemoveAt(i);
                return;
            }

            agenda.BuyMenu[i] = tuple;
        }
    }

    class SwapSupplyCardsMutation : Mutation
    {
        public override void Mutate(BuyAgenda agenda, List<Card> kingdom)
        {
            if (agenda.BuyMenu.Count == 0)
                return;

            int i = rnd.Next(agenda.BuyMenu.Count);
            int j = rnd.Next(agenda.BuyMenu.Count);

            var tuple = agenda.BuyMenu[i];
            agenda.BuyMenu[i] = agenda.BuyMenu[j];
            agenda.BuyMenu[j] = tuple;
        }
    }

    class VictoryCardPurchaseMutation : Mutation
    {
        public override void Mutate(BuyAgenda agenda, List<Card> kingdom)
        {
            int i = rnd.Next(3);

            switch (i)
            {
                case 0:
                    agenda.Estates += rnd.NextSign();
                    break;
                case 1:
                    agenda.Duchies += rnd.NextSign();
                    break;
                case 2:
                    agenda.Provinces += rnd.NextSign();
                    break;
                default:
                    break;
            }
        }
    }

    class AddCardMutation : Mutation
    {
        public override void Mutate(BuyAgenda agenda, List<Card> kingdom)
        {
            if (agenda.BuyMenu.Count == 0)
                return;

            int i = rnd.Next(agenda.BuyMenu.Count);
            int j = rnd.Next(kingdom.Count);

            agenda.BuyMenu.Insert(i, (kingdom[j].Type, rnd.Next(9) + 1));
        }
    }

    class RemoveCardMutation : Mutation
    {
        public override void Mutate(BuyAgenda agenda, List<Card> kingdom)
        {
            if (agenda.BuyMenu.Count <= 1)
                return;

            int i = rnd.Next(agenda.BuyMenu.Count);

            agenda.BuyMenu.RemoveAt(i);
        }
    }
}
