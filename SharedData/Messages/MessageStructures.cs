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
    public int targetPlayerNumber;
    public SetType chargingSetType;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PlayCardValues
{
    public bool asMoney;
    public bool addToPlayArea;  
    public int cardID; 
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct DealBreakerValues
{
    public SetType setType;
    public int targetPlayerNumber;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SlyDealvalues
{
    public int cardID;
    public int targetPlayerNumber;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ForcedDealValues
{
    public SetType givingToPlayer;
    public SetType takingFromPlayer;
    public int playerTradingWithNumber;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PlayWildCard
{
    public SetType setType;
    public int cardID;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PlayBuildingCard
{
    public SetType setType;
    public ActionType buildingType;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PayPlayer
{
    public int cardID;
    public bool asMoney;
}

