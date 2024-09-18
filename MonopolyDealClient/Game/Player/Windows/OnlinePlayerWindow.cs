using System;
using System.Collections.Generic;

namespace MonopolyDeal
{
    public class OnlinePlayerWindow : IWindow
    {
        public OnlinePlayer ConnectedPlayer { get; init; }
        public OnlinePlayerWindow(OnlinePlayer player) 
            : base(player.Name)
        {
            ConnectedPlayer = player;
        }

        public override void ImGuiDraw()
        {
            ConnectedPlayer.ImGuiDraw();
        }
    }
}
