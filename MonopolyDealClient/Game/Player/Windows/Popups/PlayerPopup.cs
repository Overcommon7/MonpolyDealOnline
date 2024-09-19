namespace MonopolyDeal
{
    public abstract class PlayerPopup : IWindow
    {
        protected Card? mCard = null;
        protected PlayerPopup(string name)
            : base(name, true, true, true) { }

        public void SetCard(Card card)
        {
            mCard = card;
        }

        public virtual void Open(Card card)
        {
            base.Open();
            SetCard(card);
        }
    }
}



