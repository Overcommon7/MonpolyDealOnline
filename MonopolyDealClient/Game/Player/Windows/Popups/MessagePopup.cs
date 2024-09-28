﻿using ImGuiNET;
using System;
using System.Collections.Generic;

namespace MonopolyDeal
{
    public class MessagePopup : IWindow
    {
        List<string> mMessages = [];
        public bool ShowCloseButton { get; set; } = false;
        public MessagePopup()
            : base("Messages", true, false, true, false)
        {
        }

        public override void ImGuiDraw()
        {
            foreach (var message in mMessages)
                ImGui.Text(message);

            if (ShowCloseButton)
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
            mMessages = new(messages);
            ShowCloseButton = showCloseButton;
            IsClosable = showCloseButton;
            base.Open();
        }

        public void ChangeMessage(string message, int messageIndex) 
        {
            if (mMessages.Count < messageIndex)
                AddMessage(message);

            mMessages[messageIndex] = message;
        }

        public void AddMessage(string message)
        {
            mMessages.Add(message);
        }
    }
}
