using ImGuiNET;
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
            mCards.Add(card);
            mCards.Sort(CardData.SortAlgorithm);

            CheckForTooManyCards();               
        }

        public void AddCards(IEnumerable<Card> cards)
        {
            mCards.AddRange(cards);
            mCards.Sort(CardData.SortAlgorithm);

            CheckForTooManyCards();
        }

        public void RemoveCard(Card card)
        {
            int index = mCards.FindIndex(c => c.ID == card.ID);
            if (index >= 0)
            {
                mCards.RemoveAt(index);
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

        void CheckForTooManyCards()
        {
            if (mCards.Count <= Constants.MAX_CARDS_IN_HAND)
                return;

            var gameplay = App.GetState<Gameplay>();
            gameplay.GetWindow<TooManyCardsPopup>().Open(gameplay.PlayerManager.LocalPlayer);
        }
    }
}


