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
        public static ulong ID { get; set; } = 0;
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

            if (!mClient.TcpClient.Connected)
                return false;

            EndPoint = mClient.TcpClient.Client.RemoteEndPoint?.ToString() ?? string.Empty;
            return true;
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
            var data = Format.GetByteDataFromMessage(e.Data);
            var message = Format.GetMessageType<ServerSendMessages>(e.Data);
            var playerNumber = Format.GetPlayerNumber(e.Data);

            Console.WriteLine($"[Client] C: {message} Number: {playerNumber}");
            mOnMessageRecieved?.Invoke(message, playerNumber, data);
        }

    }
}