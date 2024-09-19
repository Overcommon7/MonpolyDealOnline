public enum ClientSendMessages : short
{
    PlaySlyDeal,
    PlayForcedDeal,
    PlayRentCard,
    PlayWildCard,
    RequestCards,
    MoveCard,
    PayPlayer,
    OnEndTurn,
    SendUsername,
    ReadyForNextTurn,
    RequestHand
}

public enum ServerSendMessages : short
{     
    WildPropertyPlayed,
    WildCardPlayed,
    PropertyCardPlayed,
    RentCardPlayed,
    SlyDealPlayed,
    ForcedDealPlayed,
    DealBreakerPlayed,
    SingleTargetActionCard,
    ActionCardPlayed,      
    JustSayNoPlayed,

    CardsPayed,
    PlayerPaidValues,
    OnAllPlayersPaid,

    CardMoved,
    CardsSent,

    OnGameStarted,
    HandReturned,
    OnTurnStarted,
    OnPlayerWin,

    PlayerUsername,
    OnPlayerIDAssigned,
    OnPlayerConnected,
    OnPlayerDisconnected,
    OnPlayerReconnected,
}