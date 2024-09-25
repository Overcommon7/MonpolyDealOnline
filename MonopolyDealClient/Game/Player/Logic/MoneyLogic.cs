using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonopolyDeal
{
    public partial class LocalPlayer
    {
        bool NotTurn_MoneyLogic(Card card, int id)
        {
            return false;
        }

        bool RespondToAction_MoneyLogic(Card card, int id)
        {
            return false;
        }

        bool OnTurn_MoneyLogic(Card card, int id)
        {
            ImGui.SameLine();
            if (ImGui.Button($"Pay##{id}"))
               mPayPopup.AddToCardsPaying(card);

            return false;
        }
    }
}
