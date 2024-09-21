using System;
using ImGuiNET;

namespace MonopolyDeal
{
    public class DealBreakerPopup : PlayerPopup
    {
        bool mAsMoney = false;
        public DealBreakerPopup()
            : base(nameof(DealBreakerPopup))
        {
            
        }

        public override void ImGuiDraw()
        {

        }

        public override void Open()
        {
            mAsMoney = false;
            base.Open();
        }
    }
}