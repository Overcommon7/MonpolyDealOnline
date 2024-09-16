namespace MonopolyDeal
{
    public abstract class Appstate
    {
        public virtual void OnClose() { }
        public virtual void OnOpen() { }
        public virtual void Intialize() { }
        public virtual void Terminate() { }
        public virtual void ImGuiUpdate() { }
        public abstract void Update();
        public abstract void Draw();
    }
}



