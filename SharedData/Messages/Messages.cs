public enum ClientSendMessages : short
{
    PlaySlyDeal,
    PlayForcedDeal,
    PlayDealBreaker,
    PlayRentCard,
    PlayWildRentCard,
    PlayBuildingRentCard,
    PlayWildCard,
    PlayMoneyCard,
    PlayPropertyCard,
    PlayActionCard,

    ActionGotDenied,
    PaymentAccepted,
    RejectedNo,

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
    MoneyCardPlayed,
    JustSayNoPlayed,

    PlayerPaid,
    OnAllPlayersPaid,
    PaymentComplete,
    PlayerUsedSayNo,

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