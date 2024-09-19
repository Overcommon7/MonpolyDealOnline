using ImGuiNET;
using System;
using System.Collections.Generic;

namespace MonopolyDeal
{
    public class LocalPlayerWindow : IWindow
    {
        public LocalPlayer ConnectedPlayer { get; init; }
        public LocalPlayerWindow(LocalPlayer player) 
            : base(player.Name)
        {
            ConnectedPlayer = player;
        }

        public override void ImGuiDraw()
        {
            ConnectedPlayer.ImGuiDraw();

            if (!ConnectedPlayer.IsTurn)
                return;

            if (ImGui.Button("End Turn"))
                Client.SendData(ClientSendMessages.OnEndTurn, ConnectedPlayer.Number);
        }
    }
}
