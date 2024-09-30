public class WildRentCard : ActionCard, Copy<WildRentCard>
{
    public SetType SetType { get; set; } = SetType.None;
    public WildRentCard() 
        : base(ActionType.WildRent, 3)
    {
    }

    public new WildRentCard Copy()
    {
        return new WildRentCard();
    }

    public override string GetToolTip()
    {
        return "Charge Rent On Any Owned Set To A Single Player";
    }
}