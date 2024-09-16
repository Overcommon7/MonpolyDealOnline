public class BuildingCard : ActionCard, Copy<BuildingCard>
{
    public SetType CurrentSetType { get; set; }
    public int AmountToAddToSet => actionType == ActionType.Hotel ? 4 : 3;
    public bool IsHotel => actionType == ActionType.Hotel;
    public bool IsHouse => actionType == ActionType.House;
    public BuildingCard(ActionType type) 
        : base(type, type == ActionType.Hotel ? 4 : 3, false, false, false)
    {
        if (actionType != ActionType.Hotel && actionType != ActionType.House)
        {
            throw new ArgumentException($"Building Card Cannot Have Action Type {actionType}");
        }

        CurrentSetType = SetType.None;
    }

    public override string ToString()
    {
        return CurrentSetType + " - " + base.ToString();
    }

    public new BuildingCard Copy()
    {
        return new BuildingCard(actionType);
    }
}

