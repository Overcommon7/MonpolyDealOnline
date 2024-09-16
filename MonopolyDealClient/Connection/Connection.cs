using ImGuiNET;
using System.Net;
using SimpleTCP;
using System.Collections.Generic;
using System.Threading;
using System.Net.NetworkInformation;
using System;

namespace MonopolyDeal
{
    public class Connection : Appstate
    {
        string mAddress = string.Empty;
        string mPort = string.Empty;
        string mUsername = string.Empty;
        bool mValidServerCredentials = false;
        bool mIsReady = false;
        bool mAutoConnect = true;
        
        int mPlayerNumber = -1;
        public string Username => mUsername;
        public int PlayerNumber => mPlayerNumber;
        public IReadOnlyDictionary<int, (string, ulong)> OtherPlayers => mOtherPlayers;

        Dictionary<int, (string, ulong)> mOtherPlayers = new();
        public override void Draw()
        {

        }

        public override void Update()
        {
        }

        public void AddOnlinePlayer(int playerNumber, ulong id, string name)
        {
            mOtherPlayers.Add(playerNumber, (name, id));
        }

        public override void OnClose()
        {
            Client.mOnMessageRecieved -= Client_OnMessageRecieved;
        }

        public override void OnOpen()
        {
            Client.mOnMessageRecieved += Client_OnMessageRecieved;

            if (!mAutoConnect)
                return;

            mAddress = "192.168.1.86";
            mPort = "25565";
            mUsername = "Overcommon" + Random.Shared.Next(100000);

            Thread.Sleep(750);

            ConnectToServer();            
        }

        private void Client_OnMessageRecieved(ServerSendMessages message, int playerNumber, byte[] data)
        {
            if (message == ServerSendMessages.PlayerUsername)
                PlayerUsernameRecieved(playerNumber, data);

            if (message == ServerSendMessages.OnGameStarted)
                OnGameStarted(data);
        }

        private void OnGameStarted(byte[] data)
        {
            var gameplay = App.ChangeState<Gameplay>();
            var playerTurn = int.Parse(Format.ToString(data));
            gameplay.StartGame(playerTurn);
        }

        public override void ImGuiUpdate()
        {
            ImGui.Begin("Connect To Lobby");

            if (Client.IsConnected)
                CreateUsername();
            else
                EstablishConnection();

            ImGui.End();
        }

        void EstablishConnection()
        {
            bool change = false;

            change = ImGui.InputText("Server Address", ref mAddress, 30);
            change |= ImGui.InputText("Server Port:", ref mPort, 10);

            if (change)
                mValidServerCredentials = IPAddress.TryParse(mAddress, out var ipAddress) && mPort.Length >= 4;

            if (!mValidServerCredentials)
                ImGui.BeginDisabled();

            if (ImGui.Button("Connect"))
                ConnectToServer();

            if (!mValidServerCredentials)
                ImGui.EndDisabled();                        
        }
        void CreateUsername()
        {
            ImGui.TextDisabled($"Server Address: {mAddress}");
            ImGui.TextDisabled($"Server Port: {mPort}");

            bool notValid = string.IsNullOrEmpty(mUsername) || string.IsNullOrWhiteSpace(mUsername);

            if (notValid || mIsReady)
                ImGui.BeginDisabled();

            ImGui.InputText("Username", ref mUsername, 12);

            if (ImGui.Button(!mIsReady ? "Ready" : "Waiting For Other Players"))
                SendUsernameToServer();

            if (notValid || mIsReady)
                ImGui.EndDisabled();

            ImGui.SeparatorText("Players:");

            if (mIsReady)
                ImGui.Text($"You: {mUsername}");

            foreach (var player in mOtherPlayers.Values)
                ImGui.Text(player.Item1 + " - " + player.Item2);

            ImGui.TextDisabled(Client.ID.ToString());
            ImGui.TextDisabled(Client.EndPoint);
        }
        void ConnectToServer()
        {
            if (!Client.ConnectToServer(mAddress, mPort))
                return;
        }
        void SendUsernameToServer()
        {
            Client.SendData(ClientSendMessages.SendUsername, mUsername, -1);
        }

        void PlayerUsernameRecieved(int playerNumber, byte[] data)
        {
            var strs = Format.ToString(data).Split(',', System.StringSplitOptions.RemoveEmptyEntries);

            if (strs.Length != 2)
                return;

            var username = strs[0];
            var incomingID = ulong.Parse(strs[1]);

            if (incomingID == Client.ID)
            {
                mUsername = username;
                mIsReady = true;
            }
            else
            {
                if (!mOtherPlayers.TryAdd(playerNumber, (username, incomingID)))
                    mOtherPlayers[playerNumber] = (username, incomingID);
            }

        }
    }
}
