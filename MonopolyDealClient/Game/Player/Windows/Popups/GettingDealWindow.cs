using ImGuiNET;
using System;
using System.Collections.Generic;

namespace MonopolyDeal
{
    public class GettingDealWindow : IWindow
    {
        string mMessage = string.Empty;
        bool mHasSayNo = false;
        bool mIsUsingSayNo = false;
        bool mIsTarget = false;
        bool mIsReciever = false;
        bool mIsCountered = false;

        public bool ShowAcceptButton { get; set; } = false;

        Player? mReciever;
        Player? mTarget;
        public GettingDealWindow()
            : base("Getting Deal", true) { }

        
        public override void ImGuiDraw()
        {
            ImGui.Text(mMessage);

            if (mHasSayNo && mIsCountered)
            {
                if (ImGui.Button("Use Say No"))
                {

                }
            }

            if (!mHasSayNo || mIsCountered)
            {
                if (mIsTarget && ImGui.Button("Accept Deal"))
                {
                    var player = App.GetState<Gameplay>().PlayerManager.LocalPlayer;
                    Client.SendData(ClientSendMessages.DealAccepted, player.Number);
                }
            }
            
        }
        public void GotRejected()
        {
            mIsCountered = true;
            CheckForSayNo();
        }
        
        public void Open(Player reciever, Player targetPlayer, string message, int localPlayerNumber)
        {
            mReciever = reciever;
            mTarget = targetPlayer;
            mMessage = message;

            mIsReciever = reciever.Number == localPlayerNumber;
            mIsTarget = mTarget.Number == localPlayerNumber;

            mIsUsingSayNo = false;
            ShowAcceptButton = false;

            App.GetState<Gameplay>().GetWindow<LocalPlayerWindow>().IsDisabled = true;

            CheckForSayNo();
            Open();
        }

        public void CheckForSayNo()
        {
            LocalPlayer? player = null;

            if (mIsTarget)
                player = mTarget as LocalPlayer;

            if (mIsReciever)
                player = mReciever as LocalPlayer;

            if (player is null)
                return;

            mHasSayNo = player.Hand.TryGetCard<ActionCard>(card => card.ActionType == ActionType.JustSayNo, out var sayNoCard);
        }

        public override void Close()
        {
            ShowAcceptButton = false;
            App.GetState<Gameplay>().GetWindow<LocalPlayerWindow>().IsDisabled = false;
            base.Close();
        }
    }
}
