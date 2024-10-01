using ImGuiNET;
using System.Net;
using SimpleTCP;
using System.Collections.Generic;
using System.Threading;
using System;
using Raylib_cs;
using System.IO;
using rlImGui_cs;

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
        MessagePopup mMessagePopup;
        public string Username => mUsername;
        public int PlayerNumber { get; set; }
        public Texture2D ProfilePicture { get; set; }
        public IReadOnlyDictionary<int, (string, ulong, Texture2D)> OtherPlayers => mOtherPlayers;

        Dictionary<int, (string, ulong, Texture2D)> mOtherPlayers = new();

        public void AddOnlinePlayer(int playerNumber, ulong id, string name)
        {
            mOtherPlayers.TryAdd(playerNumber, (name, id, new Texture2D()));
        }

        public void RemovePlayer(int playerNumber)
        {
            mOtherPlayers.Remove(playerNumber);
        }

        public override void OnClose()
        {
            Client.mOnMessageRecieved -= Client_OnMessageRecieved;
        }

        public override void OnOpen()
        {
            Client.mOnMessageRecieved += Client_OnMessageRecieved;

#if !DEBUG
            mAddress = "154.5.153.25";
            mPort = "25565";
            mUsername = string.Empty;
            mValidServerCredentials = true;
#endif
#if DEBUG
            if (!mAutoConnect)
                return;

            mAddress = "192.168.1.100";
            mPort = "25565";
            mUsername = "Overcommon";

            Thread.Sleep(750);

            if (!Program.DebugNumber.HasValue)
                return;

            mUsername += Program.DebugNumber.Value;
            ConnectToServer();
#endif
        }

        private void Client_OnMessageRecieved(ServerSendMessages message, int playerNumber, byte[] data)
        {
            if (message == ServerSendMessages.SendConstants)
                ConstantsAssigned(data);

            if (message == ServerSendMessages.OnPlayerIDAssigned)
                IDAssigned(playerNumber, data);

            if (message == ServerSendMessages.OnPlayerConnected || message == ServerSendMessages.OnPlayerReconnected)
                PlayerConnected(playerNumber, data);

            if (message == ServerSendMessages.OnPlayerDisconnected)
                PlayerDisconnected(playerNumber, data);

            if (message == ServerSendMessages.PlayerUsername)
                PlayerUsernameRecieved(playerNumber, data);

            if (message == ServerSendMessages.OnGameStarted)
                OnGameStarted(playerNumber, data);

            //if (message == ServerSendMessages.ProfileImageSent)
            //    ProfilePictureRecieved(playerNumber, data);
        }

        private void ProfilePictureRecieved(int playerNumber, byte[] data)
        {
            if (!mOtherPlayers.ContainsKey(playerNumber))
                return;

            var image = Raylib.LoadImageFromMemory(".png", data);
            if (image.Height == 0 || image.Width == 0)
                return;

            var values = mOtherPlayers[playerNumber];
            values.Item3 = Raylib.LoadTextureFromImage(image);
            mOtherPlayers[playerNumber] = values;

            Raylib.UnloadImage(image);
        }

        private void CheckForProfilePicture()
        {
            if (mMessagePopup.IsOpen)
                return;

            if (!Raylib.IsFileDropped())
                return;

            var file = Raylib.GetDroppedFiles()[0];
            var ext = Path.GetExtension(file);
            if (ext != ".png")
                mMessagePopup.Open(["Only PNGs are accepeted"]);

            var imageData = File.ReadAllBytes(file);
            var image = Raylib.LoadImageFromMemory(ext, imageData);
            ProfilePicture = Raylib.LoadTextureFromImage(image);

            if (ProfilePicture.Id == 0)
            {
                mMessagePopup.Open(["Image Could Not Be Loaded"]);
                return;
            }

            //Client.SendData(ClientSendMessages.ProfilePictureSent, PlayerNumber);
        }

        private void ConstantsAssigned(byte[] data)
        {
            GameData.Deserialize(Format.ToString(data));
        }

        private void IDAssigned(int playerNumber, byte[] data)
        {
            if (Client.ID == 0)
            {
                var strs = Format.ToString(data).Split('|', StringSplitOptions.RemoveEmptyEntries);

                Client.ID = ulong.Parse(strs[0]);
                PlayerNumber = playerNumber;

                for (int i = 1; i < strs.Length; ++i)
                {
                    var playerData = strs[i].Split(',', StringSplitOptions.RemoveEmptyEntries);
                    if (playerData.Length != 3)
                        continue;

                    int number = int.Parse(playerData[0]);
                    ulong id = ulong.Parse(playerData[1]);
                    AddOnlinePlayer(number, id, playerData[2]);
                }
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

        private void OnGameStarted(int playerTurn, byte[] data)
        {
            CardData.LoadFromData(data);
            App.ChangeState<Gameplay>().StartingPlayerNumber = playerTurn;            
        }

        public override void ImGuiUpdate()
        {
            if (Client.IsConnected)
            {
                CreateUsername();
                if (!mMessagePopup.IsOpen)
                    return;

                mMessagePopup.ImGuiDrawBegin();
                mMessagePopup.ImGuiDraw();
                mMessagePopup.ImGuiDrawEnd();
            }                
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
            bool notValid = string.IsNullOrEmpty(mUsername) || string.IsNullOrWhiteSpace(mUsername);
            bool isDisabled = false;

            if (mIsReady)
            {
                ImGui.BeginDisabled();
                isDisabled = true;
            }
       
            ImGui.InputText("Username", ref mUsername, 12);

            if (notValid && !isDisabled)
                ImGui.BeginDisabled();

            if (ImGui.Button(!mIsReady ? "Ready" : "Waiting For Other Players"))
                SendUsernameToServer();

            if (notValid || mIsReady)
                ImGui.EndDisabled();

            ImGui.SeparatorText("You:");

            if (mIsReady)
            {
                ImGui.Text(mUsername);
                if (ProfilePicture.Id != 0)
                {
                    rlImGui.ImageSize(ProfilePicture, 250, 250);
                }
            }
                          
            //if (mIsReady)
            //    CheckForProfilePicture();

            if (ImGui.CollapsingHeader("Other Players", ImGuiTreeNodeFlags.DefaultOpen))
            {
                foreach (var player in mOtherPlayers.Values)
                {
                    ImGui.Text(player.Item1 + " - " + player.Item2);
                    if (player.Item3.Id == 0)
                        continue;

                    if (!ImGui.IsItemHovered())
                        continue;

                    ImGui.BeginTooltip();
                    rlImGui.ImageSize(player.Item3, 100, 100);
                    ImGui.EndTooltip();
                }
            }

            
            if (!ImGui.CollapsingHeader("Debug Info"))
                return;

            ImGui.TextDisabled($"Server Address: {mAddress}");
            ImGui.TextDisabled($"Server Port: {mPort}");

            ImGui.TextDisabled(Client.ID.ToString());
            ImGui.TextDisabled("Number: " + PlayerNumber.ToString());
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
            //mMessagePopup.Open(["(Optional) Drag A PNG Image Onto The Window To Set Profile Picture"]);
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
                if (!mOtherPlayers.TryAdd(playerNumber, (username, incomingID, new Texture2D())))
                    mOtherPlayers[playerNumber] = (username, incomingID, new Texture2D());
            }

        }

        public override void AddWindows()
        {
            mMessagePopup = AddWindow<MessagePopup>();
            mMessagePopup.ShowCloseButton = true;
        }
    }
}
