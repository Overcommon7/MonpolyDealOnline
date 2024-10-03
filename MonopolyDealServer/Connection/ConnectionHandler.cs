using ImGuiNET;
using SimpleTCP;
using System.Net.Sockets;

public static class ConnectionHandler
{
    public static int sReadiedPlayers = 0;
    public static bool mUseDebugConsole = true;
    public static bool mSendingProfilePictures = false;
    struct ProfileData
    {
        public static string CurrentProfileName => mProfiles[mSelectedProfileIndex];

        public static string mProfileName = string.Empty;
        public static string[] mProfiles = [];
        public static int mSelectedProfileIndex = 0;
        public const string EXTENSION = ".mdprf";

        public static void AddProfile()
        {
            var data = GameData.Serialize();
            var path = Path.Combine(Files.SaveDataDirectory, mProfileName + EXTENSION);
            File.WriteAllText(path, data);

            Array.Resize(ref mProfiles, mProfiles.Length + 1);
            mProfiles[^1] = mProfileName;
            mSelectedProfileIndex = mProfiles.Length - 1;
        }

        public static void GetAllProfilesFromFile()
        {
            var profiles = Directory.GetFiles(Files.SaveDataDirectory, '*' + EXTENSION);
            mSelectedProfileIndex = 0;

            if (profiles.Length == 0)
            {
                mProfileName = "Default";
                AddProfile();                
                return;
            }
             
            mProfiles = new string[profiles.Length];
            for (int i = 0; i < profiles.Length; i++)
                mProfiles[i] = Path.GetFileNameWithoutExtension(profiles[i]);

            mProfileName = CurrentProfileName;
        }

        public static void LoadProfileFromFile()
        {
            mProfileName = mProfiles[mSelectedProfileIndex];
            var path = Path.Combine(Files.SaveDataDirectory, mProfileName + EXTENSION);

            var data = File.ReadAllText(path);
            GameData.Deserialize(data);
        }
    }
    public static void Start()
    {
        GameManager.CurrentState = GameState.Lobby;
        sReadiedPlayers = 0;

        Server.mOnClientConnected += Server_OnClientConnected;
        Server.mOnClientDisconnected += Server_OnClientDisconnected;
        Server.mOnDataRecieved += Server_OnDataRecieved;

        ProfileData.GetAllProfilesFromFile();
    }

    public static void End()
    {
        Server.mOnDataRecieved -= Server_OnDataRecieved;
    }

    private static void Server_OnDataRecieved(ulong clientID, int playerNumber, ClientSendMessages message, byte[] data)
    {
        if (message == ClientSendMessages.SendUsername)
            UsernameRecieved(clientID, data);

        if (message == ClientSendMessages.ProfilePictureSent)
            ProfilePictureRecieved(playerNumber, data);

        if (message == ClientSendMessages.PingRequested)
            PingRequested(playerNumber);
    }

    private static void PingRequested(int playerNumber)
    {
        var status = PlayerManager.TryGetPlayer(playerNumber, out var player);
        if (status != ConnectionStatus.Connected)
            return;

        var data = Format.CreateHeader(ServerSendMessages.PingSent, playerNumber).AddDelimiter();
        player.Client.GetStream().Write(data, 0, data.Length);
    }

    private static void ProfilePictureRecieved(int playerNumber, byte[] data)
    {
        var status = PlayerManager.TryGetPlayer(playerNumber, out var player);
        if (status != ConnectionStatus.Connected)
            return;

        player.ProfilePictureData = data;
        Server.SendMessageExcluding(ServerSendMessages.ProfileImageSent, playerNumber, data, playerNumber);
    }

    static void UsernameRecieved(ulong clientID, byte[] data)
    {
        var name = Format.ToString(data);
        if (PlayerManager.TryGetPlayer(clientID, out var player) != ConnectionStatus.Connected)
            return;

        ++sReadiedPlayers;
        player.Name = name;
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

            mSendingProfilePictures = true;
            Thread.Sleep(100);

            foreach (var connected in PlayerManager.ConnectedPlayers)
            {
                if (connected == player)
                    continue;

                if (connected.ProfilePictureData.Length == 0)
                    continue;

                Console.WriteLine("[SERVER] S: Sent Image #" + connected.Number);
                var data = Format.ToData(ServerSendMessages.ProfileImageSent, connected.ProfilePictureData, connected.Number);
                client.GetStream().Write(data, 0, data.Length);
                Thread.Sleep(5000);
            }

            mSendingProfilePictures = false; 
        }  
    }

    public static void ImGuiDraw()
    {
        ImGui.Begin("Connection");

        ImGui.SeparatorText("Configuration");

        var config = GameManager.Configuration;
        bool change = ImGui.InputInt("Lobby Size", ref config.mLobbySize);
        change |= ImGui.InputInt("Decks To Play With", ref config.mDecksToUse);
        change |= ImGui.InputInt("Set To Play TO", ref config.mSetToPlayTo);

        if (change)
            GameManager.Configuration = config;

        ImGui.SeparatorText("Profile Values");
        ImGui.InputInt("Starting Cards", ref GameData.PICK_UP_AMOUNT_ON_GAME_START);
        ImGui.InputInt("Pick Up On Turn Start", ref GameData.PICK_UP_AMOUNT_ON_TURN_START);
        ImGui.InputInt("Pick Up On Empty Hand", ref GameData.PICK_UP_AMOUNT_ON_HAND_EMPTY);
        ImGui.InputInt("Max Cards In Hand", ref GameData.MAX_CARDS_IN_HAND);
        ImGui.InputInt("Max Plays Per Turn", ref GameData.MAX_PLAYS_PER_TURN);
        ImGui.InputInt("Debt Collector Amount", ref GameData.DEBT_COLLECTOR_AMOUNT);
        ImGui.InputInt("Birthday Amount", ref GameData.BIRTHDAY_AMOUNT);
        ImGui.InputInt("House Rent Increase", ref GameData.HOUSE_RENT_INCREASE);
        ImGui.InputInt("Hotel Rent Increase", ref GameData.HOTEL_RENT_INCREASE);
        ImGui.InputInt("Double Rent Multiplier", ref GameData.DOUBLE_RENT_MULTIPLIER);

        bool invalid = PlayerManager.ConnectedPlayerCount != GameManager.Configuration.mLobbySize ||
            sReadiedPlayers < GameManager.Configuration.mLobbySize || GameManager.Configuration.mLobbySize == 1;

        if (!invalid && mSendingProfilePictures)
            invalid = true;
            

        if (invalid)
        {
            ImGui.BeginDisabled();
        }

        ImGui.Separator();

        if (ImGui.Button("Start Game"))
        {
            End();
            GameManager.Start();
            var gameData = GameData.Serialize();
            Server.BroadcastMessage(ServerSendMessages.SendConstants, gameData, GameData.ALL_PLAYER_NUMBER);
            Thread.Sleep(50);

            var startingPlayerNumber = PlayerManager.ConnectedPlayers[Random.Shared.Next(0, PlayerManager.ConnectedPlayerCount)].Number;
            TurnManager.StartGame(startingPlayerNumber);

            Server.BroadcastMessage(ServerSendMessages.OnGameStarted, CardData.LoadToMemory(), startingPlayerNumber);
            Thread.Sleep(50);
            GameManager.SendInitialCards();
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

        if (ImGui.CollapsingHeader("Save/Load Profile"))
        {
            ImGui.SeparatorText("Load");
            ImGui.Combo("Load Profile", ref ProfileData.mSelectedProfileIndex, ProfileData.mProfiles, ProfileData.mProfiles.Length);
            ImGui.SameLine();
            if (ImGui.Button($"Load {ProfileData.CurrentProfileName}"))
                ProfileData.LoadProfileFromFile();
                

            ImGui.SeparatorText("Save");
            ImGui.InputText("Profile Name", ref ProfileData.mProfileName, 25);
            invalid = string.IsNullOrEmpty(ProfileData.mProfileName) || string.IsNullOrWhiteSpace(ProfileData.mProfileName) || ProfileData.mProfileName == "Default";
            
            if (invalid)
                ImGui.BeginDisabled();

            ImGui.SameLine();            
            if (ImGui.Button("Save Profile"))
                ProfileData.AddProfile();

            if (invalid)
                ImGui.EndDisabled();

            ImGui.Checkbox("Use Debug Console", ref mUseDebugConsole);
        }

        ImGui.End();
    }
}