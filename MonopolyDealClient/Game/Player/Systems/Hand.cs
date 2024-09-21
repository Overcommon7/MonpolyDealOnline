using ImGuiNET;
using System;
using System.Collections.Generic;

namespace MonopolyDeal
{
    public class Hand
    {
        List<Card> mCards;
        public IReadOnlyList<Card> Cards => mCards;
        public int NumberOfCards => mCards.Count;
        public Hand() 
        { 
            mCards = new List<Card>();
        }

        public void AddCard(Card card)
        {
            mCards.Add(card);
            mCards.Sort(CardData.SortAlgorithm);           
        }

        public void AddCards(IEnumerable<Card> cards)
        {
            mCards.AddRange(cards);
            mCards.Sort(CardData.SortAlgorithm);
        }

        public void RemoveCard(Card card, bool sort = true)
        {
            int index = mCards.FindIndex(c => c.ID == card.ID);
            if (index >= 0)
            {
                mCards.RemoveAt(index);  
                if (sort)
                    mCards.Sort(CardData.SortAlgorithm);
            }

            
        }

        public void ImGuiDraw(Action<Card>? extraLogic = null)
        {
            foreach (var card in mCards)
            {
                ImGui.Text(card.ToString());
                extraLogic?.Invoke(card);
            }
        }

        public bool CheckForTooManyCards()
        {
            if (mCards.Count <= Constants.MAX_CARDS_IN_HAND)
                return false;

            var gameplay = App.GetState<Gameplay>();
            gameplay.GetWindow<TooManyCardsPopup>().Open(gameplay.PlayerManager.LocalPlayer);
            return true;
        }

        internal void RemoveCards(List<Card> cards)
        {
            foreach (var card in cards)
                RemoveCard(card, false);

            mCards.Sort(CardData.SortAlgorithm);
        }
    }
}


