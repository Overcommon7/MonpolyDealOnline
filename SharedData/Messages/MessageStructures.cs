using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MoveValues
{
    public int cardID;
    public MoveCardType cardType;
    public SetType oldSetType;
    public SetType newSetType;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RentCardPlayValues
{
    public bool withDoubleRent;
    public int cardID;
    public SetType chargingSetType;
    public ulong chargingPlayerID;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PlayCardValues
{
    public bool asMoney;
    public bool addToPlayArea;  
    public int cardID; 
    public SetType setType;
    public ActionType actionType;
    public ulong targetPlayerID;
}

