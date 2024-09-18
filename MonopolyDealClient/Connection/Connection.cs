﻿using ImGuiNET;
using System.Net;
using SimpleTCP;
using System.Collections.Generic;
using System.Threading;
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
        public string Username => mUsername;
        public int PlayerNumber { get; set; }
        public IReadOnlyDictionary<int, (string, ulong)> OtherPlayers => mOtherPlayers;

        Dictionary<int, (string, ulong)> mOtherPlayers = new();

        public void AddOnlinePlayer(int playerNumber, ulong id, string name)
        {
            lock (mOtherPlayers)
                mOtherPlayers.TryAdd(playerNumber, (name, id));
        }

        public void RemovePlayer(int playerNumber)
        {
            lock (mOtherPlayers)
                mOtherPlayers.Remove(playerNumber);
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
            mUsername = "Overcommon";

            Thread.Sleep(750);

            if (!Program.DebugNumber.HasValue)
                return;

            mUsername += Program.DebugNumber.Value;
            ConnectToServer();            
        }

        private void Client_OnMessageRecieved(ServerSendMessages message, int playerNumber, byte[] data)
        {
            if (message == ServerSendMessages.OnPlayerIDAssigned)
                IDAssigned(playerNumber, data);

            if (message == ServerSendMessages.OnPlayerConnected || message == ServerSendMessages.OnPlayerReconnected)
                PlayerConnected(playerNumber, data);

            if (message == ServerSendMessages.OnPlayerDisconnected)
                PlayerDisconnected(playerNumber, data);

            if (message == ServerSendMessages.PlayerUsername)
                PlayerUsernameRecieved(playerNumber, data);

            if (message == ServerSendMessages.OnGameStarted)
                OnGameStarted(playerNumber);
        }

        private void IDAssigned(int playerNumber, byte[] data)
        {
            if (Client.ID == 0)
            {
                var connection = App.GetState<Connection>();
                var strs = Format.ToString(data).Split('|', StringSplitOptions.RemoveEmptyEntries);

                Client.ID = ulong.Parse(strs[0]);
                connection.PlayerNumber = playerNumber;

                for (int i = 1; i < strs.Length; ++i)
                {
                    var playerData = strs[i].Split(',', StringSplitOptions.RemoveEmptyEntries);
                    if (playerData.Length != 3)
                        continue;

                    int number = int.Parse(playerData[0]);
                    ulong id = ulong.Parse(playerData[1]);
                    connection.AddOnlinePlayer(number, id, playerData[2]);
                }
                return;
            }
        }

        private void PlayerConnected(int playerNumber, byte[] data)
        {
            if (Client.ID == 0)
                return;

            var id = ulong.Parse(Format.ToString(data));

            if (Client.ID != id)
                AddOnlinePlayer(playerNumber, id, $"Player {playerNumber}");
        }

        private void PlayerDisconnected(int playerNumber, byte[] data)
        {
            RemovePlayer(playerNumber);
        }

        private void OnGameStarted(int playerTurn)
        {
            App.ChangeState<Gameplay>().StartingPlayerNumber = playerTurn;            
        }

        public override void ImGuiUpdate()
        {

            if (Client.IsConnected)
                CreateUsername();
            else
                EstablishConnection();
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

            lock (mOtherPlayers)
            {
                foreach (var player in mOtherPlayers.Values)
                    ImGui.Text(player.Item1 + " - " + player.Item2);
            }
                
            ImGui.SeparatorText("Debug Info");

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
            var strs = Format.ToString(data).Split(',', StringSplitOptions.RemoveEmptyEntries);

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

        public override void AddWindows()
        {
            
        }
    }
}
