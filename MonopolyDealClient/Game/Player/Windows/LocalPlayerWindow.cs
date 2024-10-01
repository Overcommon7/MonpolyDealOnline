using ImGuiNET;
using rlImGui_cs;
using System;
using System.Collections.Generic;

namespace MonopolyDeal
{
    public class LocalPlayerWindow : IWindow
    {
        public LocalPlayer ConnectedPlayer { get; init; }
        public bool IsDisabled { get; set; } = false;
        public bool CanEndTurn { get; set; } = true;
        public LocalPlayerWindow(LocalPlayer player) 
            : base("You")
        {
            ConnectedPlayer = player;
        }

        public override void ImGuiDraw()
        {

            if (ConnectedPlayer.ProfilePicture.Id != 0)
            {
                ImGui.SeparatorText("Hover For Profile Picture");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    rlImGui.ImageSize(ConnectedPlayer.ProfilePicture, 150, 150);
                    ImGui.EndTooltip();
                }
            }

            if (IsDisabled || DealHandler.IsDealInProgress)
                ImGui.BeginDisabled();

            ConnectedPlayer.ImGuiDraw();

            if (ConnectedPlayer.IsTurn)
            {
                if (!IsDisabled && !DealHandler.IsDealInProgress && !CanEndTurn)
                    ImGui.BeginDisabled();

                if (ImGui.Button("End Turn"))
                {
                    var numberOfCompleteSets = ConnectedPlayer.GetNumberOfCompleteSets();

                    if (!ConnectedPlayer.Hand.CheckForTooManyCards())
                    {
                        Client.SendData(ClientSendMessages.OnEndTurn, $"{numberOfCompleteSets},{ConnectedPlayer.Hand.NumberOfCards}", ConnectedPlayer.Number);
                    }
                }

                if (!IsDisabled && !DealHandler.IsDealInProgress && !CanEndTurn)
                    ImGui.EndDisabled();
            }                

            if (IsDisabled || DealHandler.IsDealInProgress)
                ImGui.EndDisabled();

            if (ConnectedPlayer.IsTurn)
            {
                ImGui.SameLine();
                ImGui.Text($"Turns Remaining: {ConnectedPlayer.TurnsRemaining}");
            }            
        }

        
    }
}
