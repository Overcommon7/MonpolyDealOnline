namespace MonopolyDeal
{
    public abstract class Player
    {
        int mPlaysUsed = 0;
        public ulong ID { get; protected set; }
        public int Number { get; protected set; }
        public string Name { get; protected set; }
        public PlayedCards PlayedCards { get; private set; }
        public int TurnsRemaining => mPlaysUsed - GameData.MAX_PLAYS_PER_TURN;
        public bool HasPlaysRemaining => mPlaysUsed < GameData.MAX_PLAYS_PER_TURN;
        public bool IsTurn => mGameplay.PlayerManager.CurrentTurnPlayer.Number == Number;

        public int PlaysUsed
        {
            get => mPlaysUsed;
            set => mPlaysUsed = value;
        }

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

        public void StartNewTurn()
        {
            mPlaysUsed = 0;
        }

        public void EndTurn()
        {
            mPlaysUsed = 0;
        }
    }
}
