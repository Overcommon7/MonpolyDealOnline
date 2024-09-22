using System;
using ImGuiNET;

namespace MonopolyDeal
{
    public class PayPopup : PlayerPopup
    {
        int mPayAmount = 0;
        string mMessage = string.Empty;
        public PayPopup()
            : base(nameof(PayPopup))
        {
            
        }

        public override void ImGuiDraw()
        {

        }

        public void Open(string message, int value)
        {
            mMessage = message;
            mPayAmount = value;
            base.Open();
        }
    }
}