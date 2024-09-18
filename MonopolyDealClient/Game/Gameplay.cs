using System;

namespace MonopolyDeal
{
    public enum State
    {
        NotTurn,
        PlayingCards,
        RespondingToAction
    }
    public class Gameplay : Appstate
    {
        public PlayerManager? PlayerManager { get; private set; }
        public int StartingPlayerNumber { get; set; }
        public State State { get; private set; }
        
        public override void OnOpen()
        {
            PlayerManager = new PlayerManager();

            AddWindow<LocalPlayerWindow>(PlayerManager.LocalPlayer);

            foreach (var player in PlayerManager.OnlinePlayes)
                AddWindow<OnlinePlayerWindow>(player);

            StartGame();
        }
        public void StartGame()
        {
            PlayerManager.StartGame(StartingPlayerNumber);
        }

        public override void AddWindows()
        {
            
        }
    }
}
