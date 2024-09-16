using System;
using System.Collections.Generic;

namespace MonopolyDeal
{
    public class Hand
    {
        List<Card> mCards;

        public int NumberOfCards => mCards.Count;
        public Hand() 
        { 
            mCards = new List<Card>();
        }

        public void AddCard(Card card)
        {
            if (mCards.Count >= Constants.MAX_CARDS_IN_HAND)
                return;

            mCards.Add(card);
            mCards.Sort(CardData.SortAlgorithm);
        }

        public void RemoveCard(Card card)
        {
            int index = mCards.FindIndex(c => c.ID == card.ID);
            if (index >= 0)
                mCards.RemoveAt(index);

            mCards.Sort(CardData.SortAlgorithm);
        }

        public void ImGuiDraw(Action<Card>? extraLogic = null)
        {
            
        }
    }
}


