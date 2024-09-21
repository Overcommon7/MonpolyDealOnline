using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonopolyDeal
{
    public class PlayedCards
    {
        List<PropertyCard> mPropertyCards;
        List<BuildingCard> mBuildingCards;
        List<Card> mMoneyCards;

        Dictionary<SetType, int> mSetTypes;
        int playerNumber;
        
        public IReadOnlyList<PropertyCard> PropertyCards => mPropertyCards;
        public IReadOnlyList<BuildingCard> BuildingCardsCards => mBuildingCards;
        public IReadOnlyList<Card> CardsAsMoney => mMoneyCards;
        public IReadOnlyCollection<SetType> SetTypesPlayed => mSetTypes.Keys;

        public PlayedCards(Player player)
        {
            mPropertyCards = new List<PropertyCard>();
            mBuildingCards = new List<BuildingCard>();
            mMoneyCards = new List<Card>();

            playerNumber = player.Number;
            mSetTypes = new() { { SetType.None, -10000 } };
        }
        public void RemovePropertyCard(PropertyCard card)
        {
            int index = mPropertyCards.FindIndex(c => c.ID == card.ID && c.SetType == card.SetType);
            if (index == -1)
                return;

            mPropertyCards.RemoveAt(index);
            --mSetTypes[card.SetType];

            if (card.SetType != SetType.None && mSetTypes[card.SetType] == 0)
            {
                mSetTypes.Remove(card.SetType);
            }
               
        }

        public void RemoveBuildingCard(BuildingCard card)
        {
            int index = mBuildingCards.FindIndex(c => c.CurrentSetType == card.CurrentSetType && c.ActionType == card.ActionType);
            if (index == -1) 
                return;

            mBuildingCards.RemoveAt(index);
        }

        public void RemoveMoneyCard(Card card)
        {
            mMoneyCards.Remove(card);   
        }

        public void AddMoneyCard(Card card)
        {
            if (card is PropertyCard)
                return;

            if (card is ActionCard actionCard)
                actionCard.SetAsMoney(true);

            mMoneyCards.Add(card);
        }

        public void AddPropertyCard(PropertyCard card)
        {
            mPropertyCards.Add(card);
        }

        public void AddBuildingCard(BuildingCard card, SetType setType)
        {
            if (card.AsMoney)
                return;

            if (!HasFullSetOfType(setType))
                return;

            mBuildingCards.Add(card);
        }

        public bool HasPropertyCardOfType(SetType setType)
        {
            return mSetTypes.ContainsKey(setType);
        }

        public bool HasFullSetOfType(SetType setType)
        {
            if (!mSetTypes.TryGetValue(setType, out int amount))
                return false;

            return amount == CardData.GetValues(setType).AmountForFullSet;
        }

        public PropertyCard[] GetCardsOfType(SetType setType)
        {
            if (setType == SetType.None || !mSetTypes.TryGetValue(setType, out var count))
                return Array.Empty<PropertyCard>();

            PropertyCard[] cards = new PropertyCard[count];
            int index = 0;
            foreach (var card in mPropertyCards)
            {
                if (card.SetType != setType)
                    continue;

                cards[index++] = card;
                if (index >= count)
                    break;
            }

            return cards;
        }

        public void ImGuiDraw(Action<Card>? propertyLogic = null, Action<Card>? buildingLogic = null)
        {
            ImGui.TreePush($"PropertyCards##{playerNumber}");

            foreach (var setType in mSetTypes.Keys)
            {
                if (setType == SetType.None)
                    continue;

                if (!ImGui.TreeNode($"{setType}##{playerNumber}"))
                    continue;

                foreach (var card in GetCardsOfType(setType))
                {
                    ImGui.Text(card.ToString());
                    propertyLogic?.Invoke(card);
                }

                foreach (var buildingCard in mBuildingCards.FindAll(building => building.CurrentSetType == setType))
                {
                    ImGui.Text(buildingCard.ToString());
                    buildingLogic?.Invoke(buildingCard);
                }                  
            }

            ImGui.TreePop();
        }
    }
}