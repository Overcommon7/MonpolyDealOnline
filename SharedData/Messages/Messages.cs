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
    PlayBirthdayCard,
    PlayActionCard,
    ActionAgainstOne,

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
    BirthdayCardPlayed,
    DebtCollectorPlayed,
    ActionCardPlayed,
    MoneyCardPlayed,
    JustSayNoPlayed,

    PlayerPaid,
    OnAllPlayersPaid,
    PaymentComplete,
    NoWasRejected,

    CardMoved,
    CardsSent,
    UpdateCardsInHand,
    RemoveCard,

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