using System;
using System.Collections.Generic;
using System.Text;
using ImGuiNET;

namespace MonopolyDeal
{
    public class PayPopup : PlayerPopup
    {
        int mPayAmount = 0;
        bool mHasSayNo = false;
        bool mUsingSayNo = false;
        string mChargingPlayerName = string.Empty;
        string[] mMessages = [];
        LocalPlayer? mPlayer;
        List<Card> mCardsPaying;
        public PayPopup()
            : base(nameof(PayPopup)) 
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

            List<Card> notMoney = new();
            List<Card> asMoney = new();

            if (ImGui.Button($"Pay {mChargingPlayerName}"))
            {
                foreach (var card in mCardsPaying)
                {
                    if (card is BuildingCard building)
                    {
                        playedCards.RemoveBuildingCard(building);
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