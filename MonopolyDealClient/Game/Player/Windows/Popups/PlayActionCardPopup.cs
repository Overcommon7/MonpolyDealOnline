using System;
using System.Threading.Tasks;

namespace MonopolyDeal
{
    public class PlayActionCardPopup : PlayerPopup
    {
        TargetType mTargeType;
        public PlayActionCardPopup()
            : base(nameof(PlayActionCardPopup))
        {
        }

        public override void ImGuiDraw()
        {
            
        }

        public void Open(Card card, TargetType targetType)
        {
            mTargeType = targetType;
            base.Open(card);
        }
    }
}
