using ImGuiNET;
using rlImGui_cs;
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
                if (ImGui.CollapsingHeader($"{player.Name}##{player.Number}", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    if (player.ProfilePicture.Id != 0)
                    {
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            rlImGui.ImageSize(player.ProfilePicture, 150, 150);
                            ImGui.EndTooltip();
                        }
                    }
                    player.ImGuiDraw();
                }
                    
            }                
        }

        public void SetPlayers(OnlinePlayer[] connectedPlayers)
        {
            ConnectedPlayers = connectedPlayers;
        }
    }
}
