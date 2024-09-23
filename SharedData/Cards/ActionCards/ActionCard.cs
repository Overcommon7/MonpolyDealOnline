using System.Text.RegularExpressions;

public partial class ActionCard : Card, Copy<ActionCard>
{
    protected ActionType actionType;
    protected bool asMoney;
    protected bool requiresOutsideAction;
    protected bool requiresAllPlayerAction;
    protected bool requiresOnePlayerAction;
    protected string displayName;
    public ActionType ActionType => actionType;
    public bool AsMoney => asMoney;
    public bool RequiresOutsideAction => requiresOutsideAction;
    public bool RequiresAllPlayerAction => requiresOutsideAction && requiresAllPlayerAction;
    public bool RequiresOnePersonAction => requiresOutsideAction && requiresOnePlayerAction;
    public ActionCard(ActionType type, int value, bool outsideAction, bool allPlayerAction, bool onePlayerAction) 
        : base(type.ToString(), value)
    {
        actionType = type;
        asMoney = false;
        requiresOutsideAction = outsideAction;
        requiresAllPlayerAction = allPlayerAction;
        requiresOnePlayerAction = onePlayerAction;

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
        return new ActionCard(actionType, Value, requiresOutsideAction, requiresAllPlayerAction, requiresOnePlayerAction);
    }

    public override string DisplayName()
    {
        return displayName;
    }

    [GeneratedRegex("(\\B[A-Z])")]
    private static partial Regex SeperateEnums();
}

