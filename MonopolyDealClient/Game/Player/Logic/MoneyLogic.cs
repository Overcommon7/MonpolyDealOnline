using ImGuiNET;
using System;

namespace MonopolyDeal
{
    public partial class LocalPlayer
    {
        public bool CanPlayCards { get; set; } = true;

        bool NotTurn_MoneyLogic(Card card, int id)
        {
            return false;
        }

        bool RespondToAction_MoneyLogic(Card card, int id)
        {
            if (mPayPopup.IsOpen)
            {
                var isPaying = mPayPopup.IsPayingCard(card);

                if (!isPaying)
                {
                    if (mPayPopup.Value < PaymentHandler.AmountDue)
                    {
                        ImGui.SameLine();

                        if (ImGui.Button($"Pay##{id}"))
                        {
                            mPayPopup.AddToCardsPaying(card);
                        }
                    }                    
                }                    
                
                if (isPaying)
                {
                    ImGui.SameLine();
                    ImGui.BeginDisabled();
                    ImGui.Button($"In Use##{id}");
                    ImGui.EndDisabled();
                }
            }

           

            return false;
        }

        bool OnTurn_MoneyLogic(Card card, int id)
        {
            return false;
        }
    }
}
