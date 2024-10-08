﻿public class BuildingCard : ActionCard, Copy<BuildingCard>
{
    private SetType mCurrentSetType = SetType.None;
    public int AmountToAddToSet => actionType == ActionType.Hotel ? 4 : 3;
    public bool IsHotel => actionType == ActionType.Hotel;
    public bool IsHouse => actionType == ActionType.House;
    public SetType CurrentSetType 
    { 
        get => mCurrentSetType; 
        set
        {
            mCurrentSetType = value;
            mColor = CardData.GetCardColor(value);
        }
    }

    public BuildingCard(ActionType type) 
        : base(type, type == ActionType.Hotel ? 4 : 3)
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

    public override string DisplayName()
    {
        if (CurrentSetType == SetType.None)
            return displayName;

        return $"{displayName} ({CurrentSetType})";
    }

    public override string GetToolTip()
    {
        int amount = IsHouse ? GameData.HOUSE_RENT_INCREASE : GameData.HOTEL_RENT_INCREASE;
        return $"When Played On A Complete Set (Monopoly) Adds {amount} To Total Rent";
    }
}

