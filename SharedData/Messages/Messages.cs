﻿public enum ClientSendMessages : short
{
    PlaySlyDeal,
    PlayForcedDeal,
    PlayDealBreaker,
    PlayRentCard,
    PlayWildRentCard,
    PlayBuildingCard,
    PlayWildCard,
    PlayMoneyCard,
    PlayPlunderCard,
    PlayPropertyCard,
    PlayBirthdayCard,
    PlayDebtCollector,
    PlayActionCard,

    ActionGotDenied,
    PaymentAccepted,
    DealAccepted,
    RejectedNo,

    RequestCards,
    PutCardsBack,
    MoveCard,
    PayPlayer,
    OnEndTurn,
    SendUsername,
    ReadyForNextTurn,
    RequestHand,

    RecievedConstants,
    ProfilePictureSent,

    PingRequested
}

public enum ServerSendMessages : short
{     
    WildRentPlayed,
    WildCardPlayed,
    PropertyCardPlayed,
    RentCardPlayed,
    SlyDealPlayed,
    PlunderDealPlayed,
    ForcedDealPlayed,
    DealBreakerPlayed,
    BirthdayCardPlayed,
    DebtCollectorPlayed,
    BuildingCardPlayed,
    ActionCardPlayed,
    MoneyCardPlayed,
    JustSayNoPlayed,

    PlayerPaid,
    OnAllPlayersPaid,
    PaymentComplete,
    DealComplete,
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

    SendConstants,
    ProfileImageSent,
    DebugSendCard,

    PingSent,    
}