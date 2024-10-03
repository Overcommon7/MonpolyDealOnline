public class PlunderCard : ActionCard, Copy<PlunderCard>
{
    public PlunderCard(int value) 
        : base(ActionType.Plunder, value)
    {

    }

    PlunderCard Copy<PlunderCard>.Copy()
    {
        return new PlunderCard(Value);
    }

    public override string DisplayName()
    {
        return Name;
    }
}