﻿using ImGuiNET;

namespace MonopolyDeal
{
    public partial class LocalPlayer
    {
        PayPopup mPayPopup;
        bool OnTurn_PropertyLogic(Card card, int id)
        {
            if (PaymentHandler.PaymentInProcess)
                return false;

            if (!CanPlayCards)
                return false;

            if (card is WildCard)
            {
                ImGui.SameLine();
                if (ImGui.Button($"Move##{id}"))
                    mGameplay.GetWindow<MoveCardPopup>().Open(card);                                 
            }

            return false;
        }
        bool RespondToAction_PropertyLogic(Card value, int id)
        {
            if (value is not PropertyCard card)
                return false;

            ImGui.SameLine(); ImGui.Text($" - M{card.Value}"); ImGui.SameLine();
            bool invalid = PlayedCards.HasHouse(card.SetType);
            if (!invalid)
            {
                var building = PlayedCards.GetBuildingCard(ActionType.House, card.SetType);
                if (building is not null)
                    invalid = !mPayPopup.IsPayingCard(building);
            }

            if (invalid)
                ImGui.BeginDisabled();

            if (ImGui.Button($"Pay##{id}"))
                mPayPopup.AddToCardsPaying(card);

            if (invalid)
                ImGui.EndDisabled();

            return false;
        }
        bool NotTurn_PropertyLogic(Card card, int id)
        {
            return false;
        }
    }
}
