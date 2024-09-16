public class WildRentCard : ActionCard, Copy<WildRentCard>
{
    public SetType SetType { get; set; } = SetType.None;
    public WildRentCard() 
        : base(ActionType.WildRent, 3, true, false, true)
    {
    }

    public new WildRentCard Copy()
    {
        return new WildRentCard();
    }
}