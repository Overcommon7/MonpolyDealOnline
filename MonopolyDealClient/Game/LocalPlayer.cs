namespace MonopolyDeal
{
    public class LocalPlayer : Player
    {
        Hand mHand;
        public Hand Hand => mHand;
        public LocalPlayer(int playerNumber, ulong id, string name)
            : base(playerNumber, id, name) 
        { 
            mHand = new Hand();
        }



        public override void ImGuiDraw()
        {
            
        }
    }
}