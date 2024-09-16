public class ActionCard : Card, Copy<ActionCard>
{
    protected ActionType actionType;
    protected bool asMoney;
    protected bool requiresOutsideAction;
    protected bool requiresAllPlayerAction;
    protected bool requiresOnePlayerAction;
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
}

