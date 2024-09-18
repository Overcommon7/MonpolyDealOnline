namespace MonopolyDeal
{
    public abstract class PlayerPopup : IWindow
    {
        protected Card? mCard = null;
        protected PlayerPopup(string name)
            : base(name, true, true) { }

        public void SetCard(Card card)
        {
            mCard = card;
        }
    }
}



