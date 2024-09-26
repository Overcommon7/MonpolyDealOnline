using System.Text.RegularExpressions;

public partial class ActionCard : Card, Copy<ActionCard>
{
    protected ActionType actionType;
    protected bool asMoney;
    protected string displayName;
    public ActionType ActionType => actionType;
    public bool AsMoney => asMoney;
    public ActionCard(ActionType type, int value)
        : this(type, type.ToString(), value) { }
    public ActionCard(ActionType type, string name, int value, CardColor cardColor)
        : base(name, value, cardColor) 
    {
        actionType = type;
        asMoney = false;
        displayName = SeperateEnums().Replace(actionType.ToString(), " $1");
    }
    public ActionCard(ActionType type, string name, int value)
        : base(name, value, CardData.GetCardColor(type))
    {
        actionType = type;
        asMoney = false;
        displayName = SeperateEnums().Replace(actionType.ToString(), " $1");
    }
    public void SetAsMoney(bool asMoney)
    {
        this.asMoney = asMoney;
    }

    public override string ToString()
    {
        return $"{Name} - M{Value} - ID: {ID}";
    }

    public new ActionCard Copy()
    {
        return new ActionCard(actionType, Value);
    }

    public override string DisplayName()
    {
        return displayName;
    }

    [GeneratedRegex("(\\B[A-Z])")]
    private static partial Regex SeperateEnums();
}

