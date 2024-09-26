using ImGuiNET;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MonopolyDeal
{
    public class TooManyCardsPopup : IWindow
    {
        LocalPlayer? mPlayer;
        List<Card> mDiscardingCards;
        public TooManyCardsPopup() : 
            base(nameof(TooManyCardsPopup), true, true, true)
        {
            mDiscardingCards = new();
        }

        public override void ImGuiDraw()
        {
            ImGui.TextColored(Color.Red.ToVector4(), "Cards To Discard");
            for (int i = 0; i < mDiscardingCards.Count;)
            {
                ImGui.Text(mDiscardingCards[i].DisplayName());
                ImGui.SameLine();

                if (ImGui.Button("Put Back##" + i))
                    mDiscardingCards.RemoveAt(i);
                else
                    ++i;
            }

            ImGui.SeparatorText("Choose Cards To Discard");
            int suffix = 0;
            foreach (var card in mPlayer.Hand.Cards)
            {
                ++suffix;

                if (mDiscardingCards.Contains(card))
                    continue;

                ImGui.TextColored(card.Color.ToVector4(), card.DisplayName());
                ImGui.SameLine();
                if (!ImGui.Button("Discard##" + suffix))
                    continue;

                mDiscardingCards.Add(card);
                break;
            }

            bool tooManyCards = mPlayer.Hand.NumberOfCards - mDiscardingCards.Count > Constants.MAX_CARDS_IN_HAND;
            if (tooManyCards)
                ImGui.BeginDisabled();

            if (ImGui.Button("Confirm"))
            {
                mPlayer.Hand.RemoveCards(mDiscardingCards);
                Client.SendData(ClientSendMessages.PutCardsBack, Serializer.SerializeListOfCards(mDiscardingCards), mPlayer.Number);
                Thread.Sleep(100);
                Client.SendData(ClientSendMessages.OnEndTurn, mPlayer.Number);
                Close();
            }

            if (tooManyCards)
                ImGui.EndDisabled();
        }

        public void Open(LocalPlayer player)
        {
            mPlayer = player;
            base.Open();
        }

        public override void Close()
        {
            mDiscardingCards.Clear();
            base.Close();
        }
    }
}
