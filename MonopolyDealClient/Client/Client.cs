using SimpleTCP;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MonopolyDeal
{
    struct ServerRequest
    {
        public int mPlayerNumber;
        public ServerSendMessages mMessage;
        public byte[] mData;
    }
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
        private static List<ServerRequest> mRequests;
        private static bool mProcessingRequests = false;
        private static readonly object mEmptyObject = new();

        static Client()
        {
            mRequests = new();
            mClient = new SimpleTcpClient();

            mClient.Delimiter = Format.DELIMITER;
            mClient.AutoTrimStrings = false;

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

        public static void SendData(ClientSendMessages message, byte[] data, int playerNumber)
        {
            mClient.Write(Format.ToData(message, data, playerNumber));
            mOnMessageSent?.Invoke(mClient, message, playerNumber, data);
        }

        public static void SendData(ClientSendMessages message, int playerNumber)
        {
            mClient.Write(Format.CreateHeader(message, playerNumber).AddDelimiter());
            mOnMessageSent?.Invoke(mClient, message, playerNumber, mEmptyObject);
        }


        private static void Client_DataReceived(object? sender, Message e)
        {
            if (e.Data.Length < Format.HEADER_SIZE)
                return;

            ServerRequest request = new();

            request.mData = Format.GetByteDataFromMessage(e.Data);
            request.mMessage = Format.GetMessageType<ServerSendMessages>(e.Data);
            request.mPlayerNumber = Format.GetPlayerNumber(e.Data);

            if (mProcessingRequests)
            {
                Task.Run(() =>
                {
                    while (mProcessingRequests)
                        Thread.Sleep(10);

                }).Wait();
            }

            lock (mRequests)
                mRequests.Add(request);
        }

        public static void ProcessIncomingRequests()
        {
            mProcessingRequests = true;

            foreach (var request in mRequests)
                mOnMessageRecieved?.Invoke(request.mMessage, request.mPlayerNumber, request.mData);

            mRequests.Clear();

            mProcessingRequests = false;
        }
    }
}