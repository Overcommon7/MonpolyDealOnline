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
        Dictionary<SetType, ActionType> mBuildingTypes;
        int playerNumber;
        bool isLocalPlayer;
        
        public IReadOnlyList<PropertyCard> PropertyCards => mPropertyCards;
        public IReadOnlyList<BuildingCard> BuildingCards => mBuildingCards;
        public IReadOnlyList<Card> MoneyCards => mMoneyCards;
        public IReadOnlyCollection<SetType> SetTypesPlayed => mSetTypes.Keys;

        public PlayedCards(Player player)
        {
            mPropertyCards = new List<PropertyCard>();
            mBuildingCards = new List<BuildingCard>();
            mMoneyCards = new List<Card>();

            playerNumber = player.Number;
            isLocalPlayer = player is LocalPlayer;
            mBuildingTypes = new Dictionary<SetType, ActionType>();
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

            if (card.ActionType == ActionType.House)
                mBuildingTypes.Remove(card.CurrentSetType);

            if (card.ActionType == ActionType.Hotel)
                mBuildingTypes[card.CurrentSetType] = ActionType.House;

            mBuildingCards.RemoveAt(index);
        }

        public void AddBuildingCard(BuildingCard card, SetType setType)
        {
            if (card.AsMoney)
                return;

            if (!HasFullSetOfType(setType))
                return;

            if (card.ActionType == ActionType.House)
                mBuildingTypes.Add(setType, ActionType.House);

            if (card.ActionType == ActionType.Hotel)
                mBuildingTypes[setType] = ActionType.Hotel;

            card.CurrentSetType = setType;
            mBuildingCards.Add(card);
        }

        public void MoveBuildingCard(BuildingCard card, SetType setType)
        {
            RemoveBuildingCard(card);
            AddBuildingCard(card, setType);
        }

        public BuildingCard? GetBuildingCard(ActionType type, SetType setType)
        {
            return mBuildingCards.Find(card => card.ActionType == type && card.CurrentSetType == setType);
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

            if (!mSetTypes.TryAdd(card.SetType, 1))
                mSetTypes[card.SetType]++;
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

        public bool HasHouse(SetType setType)
        {
            return mBuildingTypes.ContainsKey(setType);
        }

        public bool HasHotel(SetType setType)
        {
            if (!mBuildingTypes.TryGetValue(setType, out var type))
                return false;

            return type == ActionType.Hotel;
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

        public int GetNumberOfCardsInSet(SetType setType)
        {
            if (setType == SetType.None || !mSetTypes.TryGetValue(setType, out var count))
                return 0;

            return count;
        }

        public void ImGuiDraw(
            Action<Card>? propertyLogic = null, 
            Action<Card>? buildingLogic = null, 
            Action<Card>? moneyLogic = null,
            string identifier = "1")
        {
            ImGui.SeparatorText("Properties");

            foreach (var setType in mSetTypes.Keys)
            {
                if (setType == SetType.None)
                    continue;

                if (!ImGui.TreeNode($"{setType}##{playerNumber}{identifier}"))
                    continue;

                foreach (var card in GetCardsOfType(setType))
                {
                    ImGui.Text(card.Name);
                    propertyLogic?.Invoke(card);
                }

                foreach (var buildingCard in mBuildingCards.FindAll(building => building.CurrentSetType == setType))
                {
                    ImGui.Text(buildingCard.Name);
                    buildingLogic?.Invoke(buildingCard);
                }

                ImGui.TreePop();
            }

            ImGui.SeparatorText("Money");

            if (isLocalPlayer)
            {
                if (ImGui.TreeNode($"Action Cards As Money##{playerNumber}{identifier}"))
                {
                    foreach (var card in mMoneyCards)
                    {
                        if (card is ActionCard action)
                        {
                            ImGui.Text($"M{card.Value} - {card.Name}");
                            moneyLogic?.Invoke(action);
                        }
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode($"Money Cards##{playerNumber}{identifier}"))
                {
                    foreach (var card in mMoneyCards)
                    {
                        if (card is MoneyCard money)
                        {
                            ImGui.Text($"M{card.Name}");
                            moneyLogic?.Invoke(money);
                        }                            
                    }

                    ImGui.TreePop();
                }
            }

            int value = 0;
            foreach (var card in mMoneyCards)
                value += card.Value;

            ImGui.Text($"Total: {value}");
              
        }
    }
}