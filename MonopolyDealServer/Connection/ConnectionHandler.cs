using ImGuiNET;
using SimpleTCP;
using System.Diagnostics;
using System.Net.Sockets;

public static class ConnectionHandler
{
    public static int sReadiedPlayers = 0;
    public static void Start()
    {
        GameManager.CurrentState = GameState.Lobby;
        sReadiedPlayers = 0;

        Server.mOnClientConnected += Server_OnClientConnected;
        Server.mOnClientDisconnected += Server_OnClientDisconnected;
        Server.mOnDataRecieved += Server_OnDataRecieved;
    }

    public static void End()
    {
        Server.mOnDataRecieved -= Server_OnDataRecieved;
    }

    private static void Server_OnDataRecieved(ulong clientID, ClientSendMessages message, byte[] data, Message extra)
    {
        if (message != ClientSendMessages.SendUsername)
            return;

        var name = Format.ToString(data);
        if (PlayerManager.TryGetPlayer(extra.TcpClient, out var player) != ConnectionStatus.Connected)
            return;

        Interlocked.Increment(ref sReadiedPlayers);
        Console.WriteLine("Players Ready: {0}", sReadiedPlayers);

        Server.BroadcastMessage(ServerSendMessages.PlayerUsername, name + ',' + clientID, player.Number);
    }

    private static void Server_OnClientDisconnected(SimpleTcpServer server, TcpClient client)
    {
        var status = PlayerManager.TryGetPlayer(client, out var player);
        if (status == ConnectionStatus.Invalid)
            return;
            

        PlayerManager.PlayerDisconnected(player);
        Console.WriteLine($"{player.Name} Has Disconnected");
        Server.BroadcastMessage(ServerSendMessages.OnPlayerDisconnected, player.ID.ToString(), player.Number);
    }

    private static void Server_OnClientConnected(SimpleTcpServer server, TcpClient client)
    {
        var status = PlayerManager.TryGetPlayer(client, out var player);
        if (status == ConnectionStatus.Connected)
            return;

        if (status == ConnectionStatus.Disconnected)
        {
            PlayerManager.PlayerReconnected(player);
            Console.WriteLine($"{player.Name} Has Reconnected");
            Server.BroadcastMessage(ServerSendMessages.OnPlayerReconnected, player.ID.ToString(), player.Number);
            return;
        }

        if (GameManager.CurrentState == GameState.Lobby)
        {
            player = PlayerManager.AddPlayer(client);
            Console.WriteLine($"New Client {player.ID} Has Joined - EndPoint => { client.Client.RemoteEndPoint?.ToString() }");
            Thread.Sleep(100);
            Server.BroadcastMessage(ServerSendMessages.OnPlayerConnected, player.ID.ToString(), player.Number);
        }  
    }

    public static void ImGuiDraw()
    {
        ImGui.Begin("Connection");

        var config = GameManager.Configuration;
        bool change = ImGui.InputInt("Lobby Size", ref config.mLobbySize);
        change |= ImGui.InputInt("Decks To Play With", ref config.mDecksToUse);

        if (change)
            GameManager.Configuration = config;

        bool invalid = PlayerManager.ConnectedPlayerCount != GameManager.Configuration.mLobbySize ||
            sReadiedPlayers < GameManager.Configuration.mLobbySize;

        if (invalid)
        {
            ImGui.BeginDisabled();
        }
            
  
        if (ImGui.Button("Start Game"))
        {
            End();
            GameManager.Start();

            Thread.Sleep(50);

            bool valid = false;
            for (int i = 0; i < PlayerManager.ConnectedPlayerCount; ++i)
            {
                if (PlayerManager.ConnectedPlayers[i].Name.Contains('7'))
                {
                    TurnManager.StartGame(PlayerManager.ConnectedPlayers[i].Number);
                    Server.BroadcastMessage(ServerSendMessages.OnGameStarted, CardData.LoadToMemory(), PlayerManager.ConnectedPlayers[i].Number); 
                    valid = true;
                    break;
                }
            }

            if (!valid)
            {
                var startingPlayerNumber = PlayerManager.ConnectedPlayers[Random.Shared.Next(0, PlayerManager.ConnectedPlayerCount)].Number;
                TurnManager.StartGame(startingPlayerNumber);

                Server.BroadcastMessage(ServerSendMessages.OnGameStarted, CardData.LoadToMemory(), startingPlayerNumber);
            }           
        }

        if (invalid)
        {
            ImGui.EndDisabled();
        }

        ImGui.SeparatorText("Connected Players");                           

        foreach (var player in PlayerManager.ConnectedPlayers)
        {
            ImGui.Text(player.Name + " - " + player.ID);
        }

        ImGui.End();
    }
}