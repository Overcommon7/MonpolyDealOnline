using SimpleTCP;
using System;
using System.Threading;

namespace MonopolyDeal
{
    public static class Client
    {
        static SimpleTcpClient mClient;

        public delegate void SendMessageCallback(SimpleTcpClient client, ClientSendMessages message, int playerNumber, object data);
        public delegate void RecieveMessageCallback(ServerSendMessages message, int playerNumber, byte[] data);

        public static event RecieveMessageCallback? mOnMessageRecieved;
        public static event SendMessageCallback? mOnMessageSent;

        public static bool IsConnected => mClient.TcpClient?.Connected ?? false;
        public static ulong ID { get; private set; } = 0;
        public static string EndPoint { get; private set; } = string.Empty;

        static Client()
        {
            mClient = new SimpleTcpClient();

            mClient.Delimiter = (byte)'\0';
            mClient.AutoTrimStrings = true;

            mClient.DataReceived += Client_DataReceived;
            mClient.DelimiterDataReceived += Client_DataReceived;
        }

        public static bool ConnectToServer(string ipAddress, string port)
        {
            if (!int.TryParse(port, out var portNumber))
                return false;

            mClient.Connect(ipAddress, portNumber);
            Thread.Sleep(10);

            return mClient.TcpClient.Connected;
        }

        public static void Shutdown()
        {
            mClient.Disconnect();
            mClient.Dispose();
        }

        public static void SendData<T>(ClientSendMessages message, ref T data, int playerNumber) where T : struct
        {
            mClient.Write(Format.ToData(message, ref data, playerNumber));
            mOnMessageSent?.Invoke(mClient, message, playerNumber, data);
        }

        public static void SendData(ClientSendMessages message, string data, int playerNumber)
        {
            mClient.Write(Format.ToData(message, data, playerNumber));
            mOnMessageSent?.Invoke(mClient, message, playerNumber, data);
        }

        public static void SendData(ClientSendMessages message, int playerNumber)
        {
            mClient.Write(Format.CreateHeader(message, playerNumber));
            mOnMessageSent?.Invoke(mClient, message, playerNumber, new object());
        }


        private static void Client_DataReceived(object? sender, Message e)
        {
            if (ID == 0)
            {
                var connection = App.GetState<Connection>();
                var strs = Format.ToString(e.Data).Split('|', StringSplitOptions.RemoveEmptyEntries);                
                ID = ulong.Parse(strs[0]);

                for (int i = 1; i < strs.Length; ++i)
                {
                    var playerData = strs[i].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (playerData.Length != 3)
                        continue;

                    int number = int.Parse(playerData[0]);
                    ulong id = ulong.Parse(playerData[1]);
                    connection.AddOnlinePlayer(number, id, playerData[2]);
                }
                return;
            }

            var data = Format.GetByteDataFromMessage(e.Data);
            var message = Format.GetMessageType<ServerSendMessages>(e.Data);
            var playerNumber = Format.GetPlayerNumber(e.Data);

            mOnMessageRecieved?.Invoke(message, playerNumber, data);
        }

    }
}