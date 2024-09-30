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
        int emptyWildCards;
        
        public IReadOnlyList<PropertyCard> PropertyCards => mPropertyCards;
        public IReadOnlyList<BuildingCard> BuildingCards => mBuildingCards;
        public IReadOnlyList<Card> MoneyCards => mMoneyCards;
        public IReadOnlyCollection<SetType> SetTypesPlayed => mSetTypes.Keys;
        public bool IsEmpty => mPropertyCards.Count == 0 && mBuildingCards.Count == 0 && mMoneyCards.Count == 0;
        public int TotalCardsPlayed => mPropertyCards.Count + mBuildingCards.Count + mMoneyCards.Count;


        public PlayedCards(Player player)
        {
            mPropertyCards = new List<PropertyCard>();
            mBuildingCards = new List<BuildingCard>();
            mMoneyCards = new List<Card>();

            playerNumber = player.Number;
            isLocalPlayer = player is LocalPlayer;
            mBuildingTypes = new Dictionary<SetType, ActionType>();
            mSetTypes = new() { { SetType.None, -100000 } };
            emptyWildCards = 0;
        }

        public void RemovePropertyCard(PropertyCard card)
        {
            int index = mPropertyCards.FindIndex(c => c.ID == card.ID && c.SetType == card.SetType);
            if (index == -1)
                return;

            mPropertyCards.RemoveAt(index);
            --mSetTypes[card.SetType];

            if (card.SetType != SetType.None)
            {
                if (mSetTypes[card.SetType] == 0)
                {
                    mSetTypes.Remove(card.SetType);
                }
                else if (mSetTypes[card.SetType] == 1)
                {
                    var value = GetPropertyCardsOfType(card.SetType)[0];
                    if (value.GetType() == typeof(WildCard))
                    {
                        mSetTypes.Remove(card.SetType);
                        ++emptyWildCards;
                        var wild = (WildCard)value;
                        wild.SetCurrentType(SetType.None);
                    }
                }
            }

            if (SetType.None == card.SetType)
                --emptyWildCards;
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
            if (!mMoneyCards.Remove(card))
                RemoveMoneyCard(card.ID);
        }

        public void RemoveMoneyCard(int cardID)
        {
            int index = mMoneyCards.FindIndex(card => cardID == card.ID);
            if (index == -1)
                return;

            mMoneyCards.RemoveAt(index);
        }

        public void AddMoneyCard(Card card)
        {
            if (card is PropertyCard)
                return;

            if (card is ActionCard actionCard)
                actionCard.SetAsMoney(true);

            if (!mMoneyCards.Contains(card))
                mMoneyCards.Add(card);
            else
            {
                var money = CardData.CreateNewCard<Card>(card.ID);
                if (money is not null)
                    mMoneyCards.Add(money);
            }


        }

        public void AddPropertyCard(PropertyCard card)
        {
            mPropertyCards.Add(card);

            if (!mSetTypes.TryAdd(card.SetType, 1))
                mSetTypes[card.SetType]++;

            if (SetType.None == card.SetType && card.GetType() == typeof(WildCard))
                ++emptyWildCards;
        }       

        public bool HasPropertyCardOfType(SetType setType)
        {
            return mSetTypes.ContainsKey(setType);
        }

        public bool HasFullSetOfType(SetType setType)
        {
            if (setType == SetType.None) 
                return false;

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

        public PropertyCard[] GetPropertyCardsOfType(SetType setType)
        {
            int index = 0;
            if (setType == SetType.None)
            {
                PropertyCard[] wildCards = new PropertyCard[emptyWildCards];
                foreach (var card in mPropertyCards)
                {
                    if (card is WildCard wildCard && wildCard.SetType == SetType.None)
                    {
                        wildCards[index++] = card;
                    }
                }

                return wildCards;
            }

            if (!mSetTypes.TryGetValue(setType, out var count))
                return Array.Empty<PropertyCard>();

            PropertyCard[] cards = new PropertyCard[count];            
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
            if (setType == SetType.None)
                return emptyWildCards;

            if (!mSetTypes.TryGetValue(setType, out var count))
                return 0;

            return count;
        } 
        
        public void DrawProperties(
            Func<Card, int, bool>? propertyLogic = null,
            string identifier = "1", string extra = "")
        {
            ImGui.SeparatorText("Properties" + extra);

            int id = 0;
            foreach (var setType in mSetTypes.Keys)
            {
                if (setType == SetType.None && emptyWildCards == 0)
                    continue;

                if (!ImGui.TreeNode($"{setType}##{playerNumber}{identifier}"))
                    continue;

                foreach (var card in GetPropertyCardsOfType(setType))
                {
                    ImGui.TextColored(card.Color.ToVector4(), card.Name);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text(card.GetToolTip());
                        ImGui.EndTooltip();
                    }
                    if (propertyLogic is not null && propertyLogic.Invoke(card, id++))
                        break;
                }                

                ImGui.TreePop();
            }
        }

        public void ImGuiDraw(
            Func<Card, int, bool>? propertyLogic = null,
            Func<Card, int, bool>? buildingLogic = null,
            Func<Card, int, bool>? moneyLogic = null,
            string identifier = "1")
        {
            ImGui.SeparatorText("Properties");

            int id = 0;
            foreach (var setType in mSetTypes.Keys)
            {
                if (setType == SetType.None && emptyWildCards == 0)
                    continue;

                if (!ImGui.TreeNode($"{setType}##{playerNumber}{identifier}"))
                    continue;

                foreach (var card in GetPropertyCardsOfType(setType))
                {
                    if (card is null)
                        continue;

                    ImGui.TextColored(card.Color.ToVector4(), card.Name);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text(card.GetToolTip());
                        ImGui.EndTooltip();
                    }
                    if (propertyLogic is not null && propertyLogic.Invoke(card, id++))
                        break;
                }

                foreach (var buildingCard in mBuildingCards.FindAll(building => building.CurrentSetType == setType))
                {
                    ImGui.TextColored(buildingCard.Color.ToVector4(), buildingCard.DisplayName());
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text(buildingCard.GetToolTip());
                        ImGui.EndTooltip();
                    }
                    if (buildingLogic is not null && buildingLogic.Invoke(buildingCard, id++))
                        break;
                }

                ImGui.TreePop();
            }

            int value = 0;
            foreach (var card in mMoneyCards)
                value += card.Value;

            ImGui.Spacing();
            ImGui.SeparatorText($"Money: M{value}"); 

            if (isLocalPlayer)
            {
                if (ImGui.TreeNode($"Action Cards As Money##{playerNumber}{identifier}"))
                {
                    foreach (var card in mMoneyCards)
                    {
                        if (card is ActionCard action)
                        {
                            ImGui.TextColored(card.Color.ToVector4(), $"M{card.Value} - {card.DisplayName()}");
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text(card.GetToolTip());
                                ImGui.EndTooltip();
                            }
                            if (moneyLogic is not null && moneyLogic.Invoke(action, id++))
                                break;
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
                            ImGui.TextColored(money.Color.ToVector4(), money.Name);
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text(card.GetToolTip());
                                ImGui.EndTooltip();
                            }
                            if (moneyLogic is not null && moneyLogic.Invoke(card, id++))
                                break;
                        }                            
                    }

                    ImGui.TreePop();
                }
            }                        
        }
    }
}