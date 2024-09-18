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
        public State State { get; private set; }
        
        public override void OnOpen()
        {
            PlayerManager = new PlayerManager();

            AddWindow<LocalPlayerWindow>(PlayerManager.LocalPlayer);

            foreach (var player in PlayerManager.OnlinePlayes)
                AddWindow<OnlinePlayerWindow>(player);
        }
        public void StartGame(int playerTurn)
        {
            PlayerManager.StartGame(playerTurn);
        }

        public override void AddWindows()
        {
            
        }
    }
}
