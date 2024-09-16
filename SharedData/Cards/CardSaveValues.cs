public struct PropertyCardSaveValues
{
    public string name;
    public int value;
    public SetType setType;

    public PropertyCardSaveValues()
    {
        name = string.Empty;
        value = 0;
        setType = SetType.None;
    }
}

public struct WildPropertySaveValues
{
    public PropertyCardSaveValues propertyValues;
    public SetType setType1;
    public SetType setType2;

    public WildPropertySaveValues()
    {
        propertyValues = new PropertyCardSaveValues();
        setType1 = SetType.None;
        setType2 = SetType.None;
    }
}

public struct ActionCardSaveValues
{
    public ActionType type;
    public bool requiresOutsideAction;
    public bool requiresAllPlayerAction;
    public bool requiresOnePlayerAction;
    public int value;
}

public struct RentCardSaveValues
{
    public SetType setType1;
    public SetType setType2;
}

public struct ActionValues
{
    public ActionType type;
    public int amount;
}

public struct MoneyValues
{
    public int value;
    public int amount;
}

public struct CardValues
{
    public SetType setType;
    public int amountForFullSet;
    public Rent[] prices;
}
