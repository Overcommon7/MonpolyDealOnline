using ImGuiNET;

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

            if (card is WildCard wild)
            {
                if (PlayedCards.HasHouse(wild.SetType) && PlayedCards.GetNumberOfCardsInSet(wild.SetType) == wild.AmountForFullSet)
                    return false;

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

            ImGui.SameLine(); ImGui.Text($" - M{card.Value}"); 
            bool invalid = PlayedCards.HasHouse(card.SetType);
            int count = PlayedCards.GetNumberOfCardsInSet(card.SetType);
            if (invalid && count > card.AmountForFullSet)
            {
                foreach (var property in PlayedCards.GetPropertyCardsOfType(card.SetType))
                {
                    if (mPayPopup.IsPayingCard(property))
                        --count;
                }

                if (count > card.AmountForFullSet)
                    invalid = false;
            }

            if (PlayedCards.HasHouse(card.SetType))
            {
                var building = PlayedCards.GetBuildingCard(ActionType.House, card.SetType);
                if (building is not null)
                    invalid = !mPayPopup.IsPayingCard(building);
            }

            var isPaying = mPayPopup.IsPayingCard(card);

            if (!isPaying)
            {
                if (mPayPopup.Value < PaymentHandler.AmountDue)
                {
                    ImGui.SameLine();

                    if (invalid)
                        ImGui.BeginDisabled();

                    if (ImGui.Button($"Pay##{id}"))
                        mPayPopup.AddToCardsPaying(card);

                    if (invalid)
                        ImGui.EndDisabled();
                }                
            }
            
            if (isPaying)
            {
                ImGui.SameLine();

                ImGui.BeginDisabled();
                ImGui.Button($"In Use##{id}");
                ImGui.EndDisabled();
            }

            

            return false;
        }
        bool NotTurn_PropertyLogic(Card card, int id)
        {
            return false;
        }
    }
}
