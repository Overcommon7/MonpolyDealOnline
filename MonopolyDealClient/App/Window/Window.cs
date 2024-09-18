using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonopolyDeal
{
    public abstract class IWindow
    {
        public bool IsOpen => mIsOpen;
        public bool HasMenuBar => mHasMenuBar;
        public bool IsPopup => mIsPopup;
        public bool IsClosable { get => mIsClosable; set => mIsClosable = value; }
        public string Title => mTitle;

        public IWindow(string name, bool startClosed = false, bool isClosable = false, bool isPopup = false, bool hasMenuBar = false)
        {
            if (!sTitles.TryAdd(name, 0))
                mTitle = name + "##" + sTitles[name]++;
            else
                mTitle = name;

            mIsPopup = isPopup;
            mIsOpen = !startClosed || isPopup;
            mIsClosable = isClosable || isPopup;
            mHasMenuBar = hasMenuBar && !isPopup;
        }

        public virtual void OnStateOpened() { }
        public virtual void OnStateClosed() { }
        public abstract void ImGuiDraw();
        public virtual void Update() { }
        public virtual void Draw() { }

        public virtual void Open()
        {
            mIsOpen = true;
            if (mIsPopup)
                Appstate.OpenPopup(this);    

        }
        public virtual void Close()
        {
            mIsOpen = true;
            if (mIsPopup)
                Appstate.ClosePopup();
        }
        public virtual void ImGuiDrawBegin()
        {
            if (mIsPopup)
            {
                if (!mIsOpen)
                    return;

                ImGui.OpenPopup(mTitle);
                mPopupOpenSuccessful = ImGui.BeginPopupModal(mTitle, ref mIsOpen, ImGuiWindowFlags.AlwaysAutoResize);
            }
            else
            {
                if (mHasMenuBar)
                {
                    if (mIsClosable)
                        ImGui.Begin(mTitle, ref mIsOpen, ImGuiWindowFlags.MenuBar);
                    else
                        ImGui.Begin(mTitle, ImGuiWindowFlags.MenuBar);
                }
                else if (mIsClosable)
                {
                    ImGui.Begin(mTitle, ref mIsOpen);
                }
                else
                {
                    ImGui.Begin(mTitle);
                }

            }
        }
        public virtual void ImGuiDrawEnd()
        {
            if (mIsPopup)
            {
                if (mPopupOpenSuccessful)
                {
                    ImGui.EndPopup();
                }
            }
            else
            {
                ImGui.End();
            }
        }

        string mTitle = string.Empty;
        protected bool mIsClosable = false;
        protected bool mIsOpen = true;
        protected bool mHasMenuBar = false;
        protected bool mIsPopup = false;

        private bool mPopupOpenSuccessful = false;

        static Dictionary<string, int> sTitles = new();
    }
}

