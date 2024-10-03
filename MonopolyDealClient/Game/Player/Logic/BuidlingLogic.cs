using ImGuiNET;

namespace MonopolyDeal
{
    public partial class LocalPlayer
    {
        bool OnTurn_BuildingCards(Card card, int id)
        {
            if (card is not BuildingCard building)
                return false;

            if (building.IsHouse && PlayedCards.HasHotel(building.CurrentSetType))
                return false;

            if (!CanPlayCards)
                return false;

            ImGui.SameLine();

            if (ImGui.Button($"Move##{id}"))
                mGameplay.GetWindow<MoveCardPopup>().Open(card);

            return false;
        }

        bool RespondToAction_BuildingCards(Card value, int id)
        {
            if (value is not BuildingCard card)
                return false;

            ImGui.SameLine(); ImGui.Text($" - M{card.Value}"); ImGui.SameLine();

            if (mPayPopup.IsPayingCard(card))
            {
                ImGui.BeginDisabled();
                ImGui.Button("In Use");
                ImGui.EndDisabled();
            }
            else
            {
                bool valid = true;
                if (card.ActionType == ActionType.House)
                {
                    if (PlayedCards.HasHotel(card.CurrentSetType))
                    {
                        var building = PlayedCards.GetBuildingCard(ActionType.Hotel, card.CurrentSetType);
                        if (building is not null && !mPayPopup.IsPayingCard(building))
                            valid = false;
                    }
                }

                if (!valid)
                    ImGui.BeginDisabled();

                if (ImGui.Button($"Pay##{id}"))
                    mPayPopup.AddToCardsPaying(card);

                if (!valid)
                    ImGui.EndDisabled();
            }
            
            return false;
        }

        bool NotTurn_BuildingCards(Card card, int id)
        {
            if (card is not BuildingCard building)
                return false;

            return false;
        }

    }
}