using System;
using System.Collections.Generic;
using System.Text;
using ImGuiNET;

namespace MonopolyDeal
{
    public class PayPopup : IWindow
    {
        bool mHasSayNo = false;
        bool mUsingSayNo = false;
        string[] mMessages = [];
        LocalPlayer? mPlayer;
        List<Card> mCardsPaying;
        public PayPopup()
            : base(nameof(PayPopup), true) 
        {
            mCardsPaying = new();
        }

        public void Open(LocalPlayer player, string[] messages)
        {
            mPlayer = player;
            mMessages = messages;
            mCardsPaying.Clear();

            mHasSayNo = player.Hand.TryGetCard<ActionCard>(card => card.ActionType == ActionType.JustSayNo, out var sayNoCard);
            mUsingSayNo = false;

            base.Open();
        }

        public bool IsPayingCard(Card card)
        {
            return mCardsPaying.Contains(card);
        }

        public void AddToCardsPaying(Card card)
        {
            mCardsPaying.Add(card);
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
                ImGui.TextColored(mCardsPaying[i].Color.ToVector4(), mCardsPaying[i].DisplayName());
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
            bool invalidPayment = value < PaymentHandler.AmountDue;
            if (invalidPayment && playedCards.IsEmpty)
                invalidPayment = false;

            if (invalidPayment && playedCards.TotalCardsPlayed - mCardsPaying.Count <= 0)
                invalidPayment = false;

            ImGui.Spacing();
            if (invalidPayment)
                ImGui.BeginDisabled();

            if (ImGui.Button($"Pay {PaymentHandler.PlayerNameBeingPaid}"))
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

                App.GetState<Gameplay>().RevertState();
                Client.SendData(ClientSendMessages.PayPlayer, stringBuilder.ToString(), mPlayer.Number);
                Close();
            }

            if (invalidPayment)
                ImGui.EndDisabled();
        }
    }
}