
using System.Text;

public static class Hashing
{
    private const ulong FNVOffsetBasis64 = 14695981039346656037;
    private const ulong FNVPrime64 = 1099511628211;

    private const uint FNVOffsetBasis32 = 2166136261;
    private const uint FNVPrime32 = 16777619;
    public static ulong GetHash(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        byte[] bytes = Encoding.UTF8.GetBytes(text);
        ulong hash = FNVOffsetBasis64;

        foreach (byte b in bytes)
        {
            hash ^= b;
            hash *= FNVPrime64;
        }

        return hash;
    }

    public static int GetHash32(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        byte[] bytes = Encoding.UTF8.GetBytes(text);
        uint hash = FNVOffsetBasis32;

        foreach (byte b in bytes)
        {
            hash ^= b;
            hash *= FNVPrime32;
        }

        return unchecked((int)hash);
    }
}

