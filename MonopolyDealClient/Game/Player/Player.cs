namespace MonopolyDeal
{
    public abstract class Player
    {
        public ulong ID { get; protected set; }
        public int Number { get; protected set; }
        public string Name { get; protected set; }
        public bool IsTurn => mGameplay.PlayerManager.CurrentPlayer.Number == Number;
        public PlayedCards PlayedCards { get; private set; }

        protected Gameplay mGameplay;
        public Player(int playerNumber, ulong id, string name)
        {
            ID = id; 
            Name = name;
            Number = playerNumber;
            PlayedCards = new(this);

            mGameplay = App.GetState<Gameplay>();
        }

        public abstract void ImGuiDraw();
    }
}
