using ImGuiNET;
using System.Collections.Generic;

namespace MonopolyDeal
{
    public class OnlinePlayer : Player
    {
        public int CardsInHand { get; private set; } = Constants.PICK_UP_AMOUNT_ON_GAME_START;
        public OnlinePlayer(int playerNumber, ulong id, string name)
            : base(playerNumber, id, name) { } 

        public override void ImGuiDraw()
        {
            ImGui.Begin(Name);

            ImGui.Text($"Cards In Hand: {CardsInHand}");
            PlayedCards.ImGuiDraw();            

            ImGui.End();
        }
    }
}