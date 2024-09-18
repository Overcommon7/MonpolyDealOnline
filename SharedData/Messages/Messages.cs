public enum ClientSendMessages : short
{
    PlayStealCard,
    PlayForcedDeal,
    PlayRentCard,
    PlayWildCard,
    RemoveCardsFromHand,
    RemoveCardsFromPlayArea,
    RequestCards,
    MoveCard,
    PayPlayer,
    OnTurnEnded,
    SendUsername,
    ReadyForNextTurn,
    RequestHand
}

public enum ServerSendMessages : short
{
    OnGameStarted,
    OnTurnEnded,
    CardPlayed,
    CardMoved,
    RentCardPlayed,
    ActionCardPlayed,
    CardsPayed,
    OnAllRequestsComplete,
    JustSayNoPlayed,
    OnPlayerWin,
    PlayerUsername,
    OnPlayerIDAssigned,
    OnPlayerTurnStarted,
    OnPlayerConnected,
    OnPlayerDisconnected,
    OnPlayerReconnected,
    HandReturned,
    AllPlayersHands
}