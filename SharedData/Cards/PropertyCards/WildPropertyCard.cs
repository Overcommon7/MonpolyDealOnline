public class WildPropertyCard : WildCard, Copy<WildPropertyCard>
{
    SetType setType1;
    SetType setType2;

    public SetType SetType1 => setType1;
    public SetType SetType2 => setType2;
    public bool Contains(SetType setType)
        => setType1 == setType || setType2 == setType;

    public WildPropertyCard(SetType setType1, SetType setType2, int value) 
        : base(setType1, $"{setType1} {setType2}", value)
    {
        this.setType1 = setType1;
        this.setType2 = setType2;
    }

    public override string ToString()
    {
        return $"Set Types: {setType1},{setType2} - Current: {SetType} - Value: {Value} - ID: {ID}";
    }

    public new WildPropertyCard Copy()
    {
        return new WildPropertyCard(setType1, setType2, Value);
    }

    public override void SetCurrentType(SetType setType)
    {
        if (Contains(setType))
            base.SetCurrentType(setType);
    }

    public override string DisplayName()
    {
        if (setType == SetType1)
            return $"Wild - {setType1} / {setType2}";
        return $"Wild - {setType2} / {setType1}";
    }

    public override string GetToolTip()
    {
        return $"A Property That Can Be Either {SetType1} Or {SetType2} And Can Be Swapped At Will";
    }
}

