using ImGuiNET;
using System;
using System.Collections.Generic;

namespace MonopolyDeal
{
    public class LocalPlayerWindow : IWindow
    {
        public LocalPlayer ConnectedPlayer { get; init; }
        public bool IsDisabled { get; set; } = false;
        public LocalPlayerWindow(LocalPlayer player) 
            : base(player.Name)
        {
            ConnectedPlayer = player;
        }

        public override void ImGuiDraw()
        {
            if (IsDisabled)
                ImGui.BeginDisabled();

            ConnectedPlayer.ImGuiDraw();

            if (!ConnectedPlayer.IsTurn)
                return;

            if (ImGui.Button("End Turn"))
            {
                if (!ConnectedPlayer.Hand.CheckForTooManyCards())
                {
                    Client.SendData(ClientSendMessages.OnEndTurn, ConnectedPlayer.Number);
                }                    
            }

            if (IsDisabled)
                ImGui.EndDisabled();
        }
    }
}
