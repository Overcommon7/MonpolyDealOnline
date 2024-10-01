using ImGuiNET;
using Raylib_cs;
using System.Collections.Generic;

namespace MonopolyDeal
{
    public class OnlinePlayer : Player
    {
        public int CardsInHand { get; set; } = GameData.PICK_UP_AMOUNT_ON_GAME_START;
        public OnlinePlayer(int playerNumber, ulong id, string name, Texture2D profile)
            : base(playerNumber, id, name, profile) { } 

        public override void ImGuiDraw()
        {
            ImGui.Text($"Cards In Hand: {CardsInHand}");
            PlayedCards.ImGuiDraw();            
        }
    }
}