using Raylib_cs;

namespace MonopolyDeal
{
    public abstract class Player
    {
        int mPlaysUsed = 0;
        public ulong ID { get; protected set; }
        public int Number { get; protected set; }
        public string Name { get; protected set; }
        public PlayedCards PlayedCards { get; private set; }
        public Texture2D ProfilePicture { get; protected set; }
        public int TurnsRemaining => GameData.MAX_PLAYS_PER_TURN - mPlaysUsed;
        public bool HasPlaysRemaining => mPlaysUsed < GameData.MAX_PLAYS_PER_TURN;
        public bool IsTurn => mGameplay.PlayerManager.CurrentTurnPlayer.Number == Number;

        public int PlaysUsed
        {
            get => mPlaysUsed;
            set => mPlaysUsed = value;
        }

        protected Gameplay mGameplay;
        public Player(int playerNumber, ulong id, string name, Texture2D texture)
        {
            ID = id; 
            Name = name;
            Number = playerNumber;
            ProfilePicture = texture;
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
