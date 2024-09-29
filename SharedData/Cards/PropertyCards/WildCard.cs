public class WildCard : PropertyCard, Copy<WildCard>
{
    public WildCard() 
        : base(SetType.None, "WildCard", 0)
    {
    }

    protected WildCard(SetType setType, string name, int value)
        : base(setType, name, value)
    {
    }

    public new WildCard Copy()
    {
        return new WildCard(SetType, Name, Value);
    }

    public virtual void SetCurrentType(SetType setType)
    {
        this.setType = setType;
    }

    public override string ToString()
    {
        return $"{Name} - {SetType} - M{Value} - ID: {ID}";
    }

    public override string DisplayName()
    {
        if (setType == SetType.None)
            return Name;

        return $"WildCard ({setType})";
    }
}

