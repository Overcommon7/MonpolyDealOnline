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
        
        public override void OnOpen()
        {
            PlayerManager = new PlayerManager();
        }
        public override void Draw()
        {
            
        }

        public override void Update()
        {
            
        }

        public override void ImGuiUpdate()
        {
            PlayerManager.ImGuiUpdate();
        }

        public void StartGame(int playerTurn)
        {
            PlayerManager.StartGame(playerTurn);
        }
    }
}
