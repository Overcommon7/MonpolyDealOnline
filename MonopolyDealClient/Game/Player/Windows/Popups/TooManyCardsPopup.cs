using System;
using System.Collections.Generic;

namespace MonopolyDeal
{
    public class TooManyCardsPopup : IWindow
    {
        LocalPlayer? mPlayer;
        public TooManyCardsPopup() : 
            base(nameof(TooManyCardsPopup), true, true, true)
        {
        }

        public override void ImGuiDraw()
        {
            
        }

        public void Open(LocalPlayer player)
        {
            mPlayer = player;
            base.Open();
        }
    }
}
