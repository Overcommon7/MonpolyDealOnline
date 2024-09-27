using ImGuiNET;
using System;

namespace MonopolyDeal
{
    public class MessagePopup : IWindow
    {
        string[] mMessages = Array.Empty<string>();
        bool mShowCloseButton = true;
        public MessagePopup()
            : base("Messages", true, false, true, false)
        {
        }

        public override void ImGuiDraw()
        {
            foreach (var message in mMessages)
                ImGui.Text(message);

            if (mShowCloseButton)
            {
                ImGui.Spacing();
                if (ImGui.Button("OK##MPU"))
                {
                    Close();
                }
            }
        }

        public void Open(string[] messages, bool showCloseButton = true)
        {
            mMessages = messages;
            mShowCloseButton = showCloseButton;
            IsClosable = showCloseButton;
            base.Open();
        }

        public void ChangeMessage(string message, int messageIndex) 
        {
            if (mMessages.Length < messageIndex)
                return;

            mMessages[messageIndex] = message;
        }
    }
}
