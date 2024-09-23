using ImGuiNET;

namespace MonopolyDeal
{
    public partial class LocalPlayer
    {
        bool OnTurn_PropertyLogic(Card card, int id)
        {            
            if (card is WildCard)
            {
                ImGui.SameLine();
                if (ImGui.Button($"Move##{id}"))
                    mGameplay.GetWindow<MoveCardPopup>().Open(card);                                 
            }

            return false;
        }
        bool RespondToAction_PropertyLogic(Card card, int id)
        {
            return false;
        }
        bool NotTurn_PropertyLogic(Card card, int id)
        {
            return false;
        }
    }
}
