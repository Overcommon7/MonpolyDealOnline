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
        PlayerManager? mPlayerManager;

        public int Value { get; private set; } = 0;
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
            var gameplay = App.GetState<Gameplay>();


            mPlayerManager = gameplay.PlayerManager;
            gameplay.SetToRespondingState();
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
                ImGui.TextWrapped(message);
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
            Value = 0;
            for (int i = 0; i < mCardsPaying.Count;)
            {
                ImGui.TextColored(mCardsPaying[i].Color.ToVector4(), mCardsPaying[i].DisplayName());
                ImGui.SameLine();
                if (!ImGui.Button($"Put Back##{i}"))
                {
                    Value += mCardsPaying[i].Value;
                    ++i;
                    continue;
                }

                mCardsPaying.RemoveAt(i);
            }


            var playedCards = mPlayer.PlayedCards;
            int value = 0;

            foreach (var card in playedCards.PropertyCards)
            {
                if (mCardsPaying.Contains(card))
                    continue;

                value += card.Value;
            }

            foreach (var card in playedCards.BuildingCards)
            {
                if (mCardsPaying.Contains(card))
                    continue;

                value += card.Value;
            }

            foreach (var card in playedCards.MoneyCards)
            {
                if (mCardsPaying.Contains(card))
                    continue;

                value += card.Value;
            }

            bool invalidPayment = Value < PaymentHandler.AmountDue;
            if (invalidPayment && playedCards.IsEmpty)
                invalidPayment = false;

            if (invalidPayment && playedCards.TotalCardsPlayed - mCardsPaying.Count <= 0)
                invalidPayment = false;

            if (value == 0)
                invalidPayment = false;

            ImGui.Spacing();
            if (invalidPayment)
                ImGui.BeginDisabled();

            if (ImGui.Button($"Pay {PaymentHandler.PlayerNameBeingPaid}"))
            {
                List<Card> notMoney = new();
                List<Card> asMoney = new();
                List<SetType> buildingTypes = new();
                List<SetType> wildTypes = new();

                foreach (var card in mCardsPaying)
                {
                    if (card is BuildingCard building)
                    {
                        if (building.AsMoney)
                            asMoney.Add(card);
                        else
                        {
                            buildingTypes.Add(building.CurrentSetType);
                            notMoney.Add(card);
                        }                        
                    }                     
                    else if (card is PropertyCard property)
                    {
                        if (card is WildCard wild)
                            wildTypes.Add(wild.SetType);

                        notMoney.Add(card);
                    }                      
                    else
                    {
                        asMoney.Add(card);
                    }                       
                }

                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder
                    .Append(Serializer.SerializeListOfCards(notMoney))
                    .Append('#')
                    .Append(Serializer.SerializeListOfCards(asMoney));


                stringBuilder.Append('#');
                if (buildingTypes.Count == 0)
                    stringBuilder.Append("Empty");

                for (int i = 0; i < buildingTypes.Count; i++)
                {
                    stringBuilder.Append(buildingTypes[i]);
                    if (i - 1 < buildingTypes.Count)
                        stringBuilder.Append(',');
                }

                stringBuilder.Append('#');
                if (wildTypes.Count == 0)
                    stringBuilder.Append("Empty");

                for (int i = 0; i < wildTypes.Count; i++)
                {
                    stringBuilder.Append(wildTypes[i]);
                    if (i - 1 < wildTypes.Count)
                        stringBuilder.Append(',');
                }

                App.GetState<Gameplay>().RevertState();
                Client.SendData(ClientSendMessages.PayPlayer, stringBuilder.ToString(), mPlayer.Number);
                Close();
            }

            if (invalidPayment)
                ImGui.EndDisabled();

            if (ImGui.CollapsingHeader("Other Player's Payments"))
            {
                if (mPlayerManager is not null)
                    PaymentHandler.ImGuiDraw(mPlayerManager, null);
            }                
        }
    }
}