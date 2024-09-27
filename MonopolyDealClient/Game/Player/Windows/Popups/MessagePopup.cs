using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonopolyDeal
{
    public class MessagePopup : IWindow
    {
        public MessagePopup()
            : base("Messages", true, false, true, false)
        {
        }

        public override void ImGuiDraw()
        {
            
        }
    }
}
