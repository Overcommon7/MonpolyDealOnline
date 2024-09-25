using ImGuiNET;
using System;
using System.Collections.Generic;

namespace MonopolyDeal
{
    public class GettingPaidWindow : IWindow
    {
        Gameplay mGameplay;
        bool mHasSayNo;
        int id = 0;
        public GettingPaidWindow() 
            : base("Payment Details", true)
        {
            mGameplay = App.GetState<Gameplay>();
        }

        public override void Open()
        {
            var player = mGameplay.PlayerManager.LocalPlayer;
            mHasSayNo = player.Hand.TryGetCard<ActionCard>(card => card.ActionType == ActionType.JustSayNo, out var card);
            base.Open();
        }

        public override void ImGuiDraw()
        {
            id = 0;
            PaymentHandler.ImGuiDraw(mGameplay.PlayerManager, SayNoLogic);
            ImGui.Spacing();

            if (PaymentHandler.AllPlayersPaid)
            {
                if (ImGui.Button("AcceptPayment"))
                {
                    Client.SendData(ClientSendMessages.PaymentAccepted, mGameplay.PlayerManager.LocalPlayer.Number);
                    Close();
                }
            }
            else
            {
                ImGui.BeginDisabled();
                ImGui.Button("Waiting For Payments");
                ImGui.EndDisabled();
            }

            ImGui.SeparatorText("Debug");
            if (!ImGui.Button("Refresh Say No"))
            {
                mHasSayNo = mGameplay.PlayerManager.LocalPlayer.
                    Hand.TryGetCard<ActionCard>(card => card.ActionType == ActionType.JustSayNo, out var card);
            }
        }

        void SayNoLogic(int playerNumber)
        {
            if (mGameplay.State != State.PlayingCards)
                return;

            if (!mHasSayNo)
                return;

            ImGui.SameLine();
            if (!ImGui.Button("Counter Say No"))
                return;

            var player = mGameplay.PlayerManager.LocalPlayer;

            player.Hand.TryGetCard<ActionCard>(card => card.ActionType == ActionType.JustSayNo, out var card);
            player.Hand.RemoveCard(card);

            mHasSayNo = player.Hand.TryGetCard<ActionCard>(card => card.ActionType == ActionType.JustSayNo, out card);
            Client.SendData(ClientSendMessages.RejectedNo, playerNumber);
        }
    }
}
