using System.Net;
using System.Net.Sockets;

public static class Extentions
{
    public static ulong GetID(this TcpClient client)
    {
        string address = Server.Address?.ToString() ?? throw new ArgumentNullException();

        string? endpoint = client.Client.RemoteEndPoint?.ToString();
        if (string.IsNullOrEmpty(endpoint))
            return 0;

        string sub = endpoint.Substring(0, endpoint.IndexOf(':') + 1);
        if (sub == address) 
            return Hashing.GetHash(endpoint);

        return Hashing.GetHash(sub);
    }
}