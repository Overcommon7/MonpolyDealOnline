using System;
using System.Collections.Generic;
using System.Text;
using ImGuiNET;

namespace MonopolyDeal
{
    public class PayPopup : IWindow
    {
        int mPayAmount = 0;
        bool mHasSayNo = false;
        bool mUsingSayNo = false;
        string mChargingPlayerName = string.Empty;
        string[] mMessages = [];
        LocalPlayer? mPlayer;
        List<Card> mCardsPaying;
        public PayPopup()
            : base(nameof(PayPopup), true) 
        {
            mCardsPaying = new();
        }

        public void Open(LocalPlayer player, string chargingPlayerName, string[] messages, int value)
        {
            mPlayer = player;
            mChargingPlayerName = chargingPlayerName;
            mMessages = messages;
            mPayAmount = value;
            mCardsPaying.Clear();

            mHasSayNo = player.Hand.TryGetCard<ActionCard>(card => card.ActionType == ActionType.JustSayNo, out var sayNoCard);
            mUsingSayNo = false;

            base.Open();
        }

        public override void ImGuiDraw()
        {
            if (mPlayer is null)
                return;

            foreach (var message in mMessages)
            {
                ImGui.Text(message);
            }

            if (mHasSayNo)
            {
                ImGui.Checkbox("Use Just Say No##PayPU", ref mUsingSayNo);
                if (mUsingSayNo)
                {
                    if (ImGui.Button("Use##PayPU"))
                    {
                        if (!CardData.TryGetCard<ActionCard>(card => card.ActionType == ActionType.JustSayNo, out var sayNoCard))
                            return;

                        mPlayer.Hand.RemoveCard(sayNoCard);
                        Client.SendData(ClientSendMessages.ActionGotDenied, mPlayer.Number);
                        Close();
                    }

                    return;
                }    
            }

            ImGui.Spacing();
            ImGui.SeparatorText("Cards Paying");
            int value = 0;
            for (int i = 0; i < mCardsPaying.Count;)
            {
                ImGui.Text(mCardsPaying[i].DisplayName());
                ImGui.SameLine();
                if (!ImGui.Button($"Put Back##{i}"))
                {
                    value += mCardsPaying[i].Value;
                    ++i;
                    continue;
                }

                mCardsPaying.RemoveAt(i);
            }

            var playedCards = mPlayer.PlayedCards;
            ImGui.Spacing();
            playedCards.ImGuiDraw(PropertyLogic, BuildingLogic, MoneyLogic, "Pay");

            ImGui.Spacing();
            if (value < mPayAmount)
                ImGui.BeginDisabled();

            if (ImGui.Button($"Pay {mChargingPlayerName}"))
            {
                List<Card> notMoney = new();
                List<Card> asMoney = new();
                List<SetType> buildingTypes = new();

                foreach (var card in mCardsPaying)
                {
                    if (card is BuildingCard building)
                    {
                        playedCards.RemoveBuildingCard(building);
                        buildingTypes.Add(building.CurrentSetType);
                        notMoney.Add(card);
                    }                     
                    else if (card is PropertyCard property)
                    {
                        playedCards.RemovePropertyCard(property);
                        notMoney.Add(card);
                    }                      
                    else
                    {
                        playedCards.RemoveMoneyCard(card);
                        asMoney.Add(card);
                    }                       
                }

                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder
                    .Append(Serializer.SerializeListOfCards(notMoney))
                    .Append('#')
                    .Append(Serializer.SerializeListOfCards(asMoney));

                if (buildingTypes.Count > 0)
                {
                    stringBuilder.Append('#');
                    for (int i = 0; i < buildingTypes.Count; i++)
                    {
                        stringBuilder.Append(buildingTypes[i]);
                        if (i - 1 < buildingTypes.Count)
                            stringBuilder.Append(',');
                    }
                }

                Client.SendData(ClientSendMessages.PayPlayer, stringBuilder.ToString(), mPlayer.Number);
                Close();
            }

            if (value < mPayAmount)
                ImGui.EndDisabled();

#region DrawFunctions

            bool PropertyLogic(Card value, int id)
            {
                if (value is not PropertyCard card)
                    return false;

                ImGui.SameLine(); ImGui.Text($" - M{card.Value}"); ImGui.SameLine();
                bool invalid = playedCards.HasHouse(card.SetType);
                if (!invalid)
                {
                    var building = playedCards.GetBuildingCard(ActionType.House, card.SetType);
                    if (building is not null)
                        invalid = !mCardsPaying.Contains(building);
                }

                if (invalid)
                    ImGui.BeginDisabled();

                if (ImGui.Button($"Pay##{id}"))
                    mCardsPaying.Add(card);

                if (invalid)
                    ImGui.EndDisabled();

                return false;
            }

            bool BuildingLogic(Card value, int id)
            {
                if (value is not BuildingCard card)
                    return false;

                ImGui.SameLine(); ImGui.Text($" - M{card.Value}"); ImGui.SameLine();
                bool valid = true;
                if (card.ActionType == ActionType.House)
                {
                    if (playedCards.HasHotel(card.CurrentSetType))
                    {
                        var building = playedCards.GetBuildingCard(ActionType.Hotel, card.CurrentSetType);
                        if (building is not null && !mCardsPaying.Contains(building))
                            valid = false;
                    }
                }

                if (!valid)
                    ImGui.BeginDisabled();

                if (ImGui.Button($"Pay##{id}"))
                    mCardsPaying.Add(card);

                if (!valid)
                    ImGui.EndDisabled();

                return false;

            }

            bool MoneyLogic(Card card, int id)
            {
                ImGui.SameLine();
                if (ImGui.Button($"Pay##{id}"))
                    mCardsPaying.Add(card);

                return false;
            }

#endregion
        }
    }
}