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

        public bool TryGetCard<T>(Predicate<T> predicate, out T card) where T : Card
        {
            int index = mCards.FindIndex(value =>
            {
                if (value is not T typedCard)
                    return false;

                return predicate(typedCard);
            });

            if (index == -1)
            {
                card = null;
                return false;
            }
               
            card = mCards[index] as T;
            return true;
        }
        
        public void ImGuiDraw(Func<Card, int, bool>? extraLogic = null)
        {
            for (int i = 0; i < mCards.Count; i++)
            {
                ImGui.TextColored(mCards[i].Color.ToVector4(), mCards[i].DisplayName());
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text(mCards[i].GetToolTip());
                    ImGui.EndTooltip();
                }

                if (extraLogic is not null && extraLogic.Invoke(mCards[i], i))
                    return;
            }
        }

        public bool CheckForTooManyCards()
        {
            if (mCards.Count <= GameData.MAX_CARDS_IN_HAND)
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


