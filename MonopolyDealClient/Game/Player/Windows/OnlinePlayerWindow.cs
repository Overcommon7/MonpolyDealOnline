using ImGuiNET;
using System;
using System.Collections.Generic;

namespace MonopolyDeal
{
    public class OnlinePlayerWindow : IWindow
    {
        public OnlinePlayer[] ConnectedPlayers { get; init; }
        public OnlinePlayerWindow(params OnlinePlayer[] players) 
            : base("Online Players")
        {
            ConnectedPlayers = players;
        }

        public override void ImGuiDraw()
        {
            foreach (var player in ConnectedPlayers)
            {
                if (ImGui.CollapsingHeader($"{player.Name}##{player.Number}"))
                    player.ImGuiDraw();
            }
                
        }
    }
}
