using ImGuiNET;

namespace MonopolyDeal
{
    public partial class LocalPlayer
    {
        void OnTurn_PropertyLogic(Card card)
        {            
            if (card is WildCard)
            {
                ImGui.SameLine();
                if (ImGui.Button("Move"))
                    mGameplay.GetWindow<MoveCardPopup>().Open(card);                                 
            }
        }
        void RespondToAction_PropertyLogic(Card card)
        {

        }
        void NotTurn_PropertyLogic(Card card)
        {

        }
    }
}
