using ImGuiNET;

namespace MonopolyDeal
{
    public partial class LocalPlayer
    {
        void OnTurn_PropertyLogic(Card card)
        {            
            if (card is WildCard property)
            {
                ImGui.SameLine();
                if (ImGui.Button("Move"))
                    mGameplay.GetWindow<MoveCardPopup>();
                return;
            }
                

            


        }
        void RespondToAction_PropertyLogic(Card card)
        {
            if (card is not PropertyCard property)
                return;


        }
        void NotTurn_PropertyLogic(Card card)
        {
            if (card is not PropertyCard property)
                return;


        }
    }
}
