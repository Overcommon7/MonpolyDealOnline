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
            : base("You")
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
                var numberOfCompleteSets = ConnectedPlayer.GetNumberOfCompleteSets();

                if (!ConnectedPlayer.Hand.CheckForTooManyCards())
                {
                    Client.SendData(ClientSendMessages.OnEndTurn, $"{numberOfCompleteSets},{ConnectedPlayer.Hand.NumberOfCards}", ConnectedPlayer.Number);
                }                    
            }

            if (IsDisabled)
                ImGui.EndDisabled();
        }

        
    }
}
