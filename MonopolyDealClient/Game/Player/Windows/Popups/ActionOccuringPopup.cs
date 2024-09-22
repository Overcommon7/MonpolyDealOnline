using System;
using System.Collections.Generic;
using ImGuiNET;

namespace MonopolyDeal
{
    public class ActionOccuringPopup : IWindow
    {
        string mMessage = string.Empty;
        public ActionOccuringPopup()
            : base(nameof(ActionOccuringPopup), true, true, true) { }

        public override void ImGuiDraw()
        {
            ImGui.Text(mMessage);
        }

        public void Open(string message)
        {
            mMessage = message;
            base.Open();
        }
    }
}
