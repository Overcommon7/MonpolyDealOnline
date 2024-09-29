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
public struct WildRentPlayValues
{
    public bool withDoubleRent; 
    public int cardID;
    public int targetPlayerNumber;
    public int cardsOwnedInSet;
    public SetType chargingSetType;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RentPlayValues
{
    public bool withDoubleRent;
    public int cardID;
    public int cardsOwnedInSet;
    public SetType chargingSetType;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PlayActionCardValues
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
public struct SlyDealValues
{
    public int cardID;
    public int targetPlayerNumber;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ForcedDealValues
{
    public int givingToPlayerID;
    public int takingFromPlayerID;
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
    public int cardID;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct DebtCollectorValues
{
    public ActionType actionType;
    public int targetPlayerNumber;
}

