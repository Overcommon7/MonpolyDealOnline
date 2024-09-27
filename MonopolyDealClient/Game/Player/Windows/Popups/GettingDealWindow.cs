using ImGuiNET;
using System;
using System.Collections.Generic;

namespace MonopolyDeal
{
    public class GettingDealWindow : IWindow
    {
        string mMessage = string.Empty;
        bool mHasSayNo = false;
        LocalPlayer? mPlayer;
        OnlinePlayer? mTargetPlayer;
        public GettingDealWindow()
            : base("Getting Paid", true, false, false, false) { }

        
        public override void ImGuiDraw()
        {
            ImGui.Text(mMessage);

        }

        public void Open(LocalPlayer player, OnlinePlayer targetPlayer, string message)
        {
            mPlayer = player;
            mTargetPlayer = targetPlayer;
            mMessage = message;
            Open();
        }

        public void CheckForSayNo()
        {

        }
    }
}
