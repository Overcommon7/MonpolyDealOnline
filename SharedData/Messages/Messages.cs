public enum ClientSendMessages : short
{
    PlayCard,
    PlayRentCard,
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
    OnPlayerDisconnected,
    OnPlayerReconnected,
    HandReturned,
    AllPlayersHands
}