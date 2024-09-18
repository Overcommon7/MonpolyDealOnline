using System.Net;
using System.Net.Sockets;

public static class Extentions
{
    public static ulong GetID(this TcpClient client)
    {
        //string address = Server.Address?.ToString() ?? throw new ArgumentNullException();

        string? endpoint = client.Client.RemoteEndPoint?.ToString();
        if (string.IsNullOrEmpty(endpoint))
            return 0;

        //int index = endpoint.IndexOf(':');

        //if (index != -1)
        //{
        //    string sub = endpoint.Substring(0, index);
        //    if (sub != address)
        //        return Hashing.GetHash(sub);
        //}
        
        return Hashing.GetHash(endpoint);

      
    }
}