public enum ClientSendMessages : short
{
    PlaySlyDeal,
    PlayForcedDeal,
    PlayRentCard,
    PlayWildCard,
    RequestCards,
    PutCardsBack,
    MoveCard,
    PayPlayer,
    OnEndTurn,
    SendUsername,
    ReadyForNextTurn,
    RequestHand
}

public enum ServerSendMessages : short
{     
    WildRentPlayed,
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
    UpdateCardsInHand,

    OnGameStarted,
    HandReturned,
    OnTurnStarted,
    OnPlayerWin,

    PlayerUsername,
    OnPlayerIDAssigned,
    OnPlayerConnected,
    OnPlayerDisconnected,
    OnPlayerReconnected,

    DebugSendCard
}