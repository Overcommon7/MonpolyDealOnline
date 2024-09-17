namespace MonopolyDeal
{
    public abstract class Player
    {
        public ulong ID { get; protected set; }
        public int Number { get; protected set; }
        public string Name { get; protected set; }
        public bool IsTurn => App.GetState<Gameplay>().PlayerManager.CurrentPlayer.Number == Number;
        public PlayedCards PlayedCards { get; private set; }
        public Player(int playerNumber, ulong id, string name)
        {
            ID = id; 
            Name = name;
            Number = playerNumber;
            PlayedCards = new(this);
        }

        public abstract void ImGuiDraw();
    }
}
