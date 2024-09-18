namespace MonopolyDeal
{
    public abstract class PlayerPopup : IWindow
    {
        protected Card? mCard = null;
        protected PlayerPopup(string name)
            : base(name, true, true) { }
    }
}



