using ImGuiNET;
using System;
using System.Collections.Generic;

namespace MonopolyDeal
{
    public class OnlinePlayersWindow : IWindow
    {
        public OnlinePlayer[] ConnectedPlayers { get; private set; }
        public OnlinePlayersWindow() 
            : base("Online Players")
        {
            ConnectedPlayers = [];
        }

        public override void ImGuiDraw()
        {
            foreach (var player in ConnectedPlayers)
            {
                if (ImGui.CollapsingHeader($"{player.Name}##{player.Number}"))
                    player.ImGuiDraw();
            }                
        }

        public void SetPlayers(OnlinePlayer[] connectedPlayers)
        {
            ConnectedPlayers = connectedPlayers;
        }
    }
}
