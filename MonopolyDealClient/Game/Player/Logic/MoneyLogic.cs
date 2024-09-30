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
            ImGui.SameLine();

            if (mPayPopup.IsOpen)
            {
                var isPaying = mPayPopup.IsPayingCard(card);

                if (!isPaying)
                {
                    if (ImGui.Button($"Pay##{id}"))
                    {
                        mPayPopup.AddToCardsPaying(card);
                    }
                }                    
                else
                {
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
