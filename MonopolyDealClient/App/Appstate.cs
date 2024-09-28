using System.Collections.Generic;
using System;

namespace MonopolyDeal
{
    public abstract class Appstate
    {
        private List<IWindow> mWindows = new();
        public abstract void AddWindows();
        protected T AddWindow<T>(params object?[]? values) where T : IWindow
        {
            T? window = Activator.CreateInstance(typeof(T), values) as T;
            if (window is null)
                throw new ArgumentNullException(nameof(window));

            mWindows.Add(window);
            return window;
        }
        public virtual void Intialize() { }
        public virtual void Terminate() { }
        public virtual void Draw() { }
        public virtual void Update() { }
        public virtual void OnClose() 
        {
            foreach (var window in mWindows)
            {
                window.OnStateClosed();
            }
        }
        public virtual void OnOpen() 
        {
            foreach (var window in mWindows)
            {
                window.OnStateOpened();
            }
        }
        public virtual void ImGuiUpdate() 
        {
            foreach (var window in mWindows)
            {
                if (!window.IsOpen)
                    continue;

                if (window.IsPopup)
                {
                    if (CurrentPopup is null)
                        continue;

                    if (CurrentPopup != window)
                        continue;
                }


                window.ImGuiDrawBegin();
                window.ImGuiDraw();
                window.ImGuiDrawEnd();
            }
        }
        public T GetWindow<T>() where T : IWindow
        {
            var type = typeof(T);
            foreach (var window in mWindows)
            {
                if (window.GetType() == type)
                    return (T)window;
            }

            throw new InvalidOperationException("Window Does Not Exist");
        }

        public T GetWindow<T>(Predicate<IWindow> match) where T : IWindow
        {
            foreach (var window in mWindows)
            {
                if (window is not T value)
                    continue;

                if (match(value))
                    return value;
            }

            throw new InvalidOperationException("Window Does Not Exist");
        }
        public static void OpenPopup(IWindow popup)
        {
            CurrentPopup = popup;
        }
        public static void ClosePopup()
        {
            sPopupStack.TryPop(out var popup);
        }

        public static IWindow? CurrentPopup
        {
            get => sPopupStack.Count > 0 ? sPopupStack.Peek() : null;
            private set
            {
                if (value is null)
                    throw new NullReferenceException();

                sPopupStack.Push(value);
            }
        }

        static Stack<IWindow> sPopupStack = new Stack<IWindow>();
        
    }
}



