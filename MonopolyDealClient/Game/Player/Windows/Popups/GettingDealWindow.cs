using ImGuiNET;
using System;
using System.Collections.Generic;

namespace MonopolyDeal
{
    public class GettingDealWindow : IWindow
    {
        string mMessage = string.Empty;
        bool mHasSayNo = false;
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
                if (ImGui.Button("Use Say No##GDPU"))
                {
                    var player = GetLocalPlayer();

                    if (player is null)
                        return;

                    player.Hand.TryGetCard<ActionCard>(card => card.ActionType == ActionType.JustSayNo, out var justSayNo);
                    player.Hand.RemoveCard(justSayNo);

                    CheckForSayNo();
                    mIsCountered = false;

                    if (mIsReciever && mReciever is not null)
                    {
                        Client.SendData(ClientSendMessages.RejectedNo, player.Number);
                    }

                    if (mIsTarget)
                    {                        
                        Client.SendData(ClientSendMessages.ActionGotDenied, player.Number);
                    }
                }
            }


            if (mIsTarget && ImGui.Button("Accept Deal"))
            {
                var player = App.GetState<Gameplay>().PlayerManager.LocalPlayer;
                Client.SendData(ClientSendMessages.DealAccepted, player.Number);
                Close();
            }

            if (mIsReciever && ShowAcceptButton)
            {
                if (ImGui.Button("Confirm##GHS"))
                {
                    Close();
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

            ShowAcceptButton = false;

            mIsCountered = mIsTarget;

            //App.GetState<Gameplay>().GetWindow<LocalPlayerWindow>().IsDisabled = true;

            CheckForSayNo();
            Open();
        }
        LocalPlayer? GetLocalPlayer()
        {
            LocalPlayer? player = null;

            if (mIsTarget)
                player = mTarget as LocalPlayer;

            if (mIsReciever)
                player = mReciever as LocalPlayer;

            return player;
        }
        public void CheckForSayNo()
        {
            var player = GetLocalPlayer();

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

        public void ChangeMessage(string message)
        {
            mMessage = message;
        }
    }
}
