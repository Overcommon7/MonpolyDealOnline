using System.Runtime.InteropServices;
using System.Text;

public static class Format
{
    public const int MESSAGE_SIZE = 26;
    public const int PLAYER_ID_LENGTH = 14;
    public const int HEADER_SIZE = MESSAGE_SIZE + PLAYER_ID_LENGTH;
    public static byte[] CreateHeader<T>(T messageName) where T : struct, Enum
    {
        var header = messageName.ToString().PadRight(HEADER_SIZE);
        return Encoding.UTF8.GetBytes(header);
    }

    public static byte[] CreateHeader<T>(T messageName, int playerNumber) where T : struct, Enum
    {
        var header = messageName.ToString().PadRight(MESSAGE_SIZE);
        header += CreatePlayerIDString(playerNumber);
        return Encoding.UTF8.GetBytes(header);
    }

    public static bool ContainsProperlyFormattedHeader<T>(byte[] data) where T : struct, Enum
    {
        string header = Encoding.UTF8.GetString(data, 0, MESSAGE_SIZE);
        if (!Enum.TryParse<T>(header.TrimEnd(), false, out T result))
            return false;

        string id = Encoding.UTF8.GetString(data, MESSAGE_SIZE, PLAYER_ID_LENGTH);
        return int.TryParse(id.TrimEnd(), out int number);
    }

    public static string CreateHeaderString<T>(T messageName, int playerNumber) where T : struct, Enum
    {
        var header = messageName.ToString().PadRight(MESSAGE_SIZE);
        return header + CreatePlayerIDString(playerNumber);        
    }

    public static byte[] Encode(string fullmessage)
    {
        return Encoding.UTF8.GetBytes(fullmessage);
    }

    public static string CreatePlayerIDString(int playerNumber)
    {
        return playerNumber.ToString().PadRight(PLAYER_ID_LENGTH);
    }

    public static byte[] ToData<T1, T2>(T1 message, ref T2 data) 
        where T1 : struct, Enum where T2 : struct
    {       
        var structure = StructToByteArray(ref data);
        var header = CreateHeader(message);

        return CombineByteArrays(header, structure);
    }

    public static byte[] ToData<T1, T2>(T1 message, ref T2 data, int playerNumber)
        where T1 : struct, Enum where T2 : struct
    {
        var structure = StructToByteArray(ref data);
        var header = CreateHeader(message, playerNumber);

        return CombineByteArrays(header, structure);
    }

    public static byte[] ToData<T1, T2>(T1 message, T2[] data, int playerNumber)
        where T1 : struct, Enum where T2 : struct
    {
        var structures = MarshalStructArrayToByteArray(data);
        var header = CreateHeader(message, playerNumber);

        return CombineByteArrays(header, structures);
    }

    public static byte[] ToData<T>(T message, string data, int playerNumber)
        where T : struct, Enum
    {
        var header = CreateHeader(message, playerNumber);
        var content = Encode(data);

        return CombineByteArrays(header, content);
    }

    public static byte[] ToData<T>(T message, byte[] data, int playerNumber)
       where T : struct, Enum
    {
        var header = CreateHeader(message, playerNumber);
        return CombineByteArrays(header, data);
    }

    public static T ToStruct<T>(byte[] data, bool parseFromMessage = false) where T : struct
    {
        if (parseFromMessage)
        {
            var result = GetByteDataFromMessage(data);
            return ByteArrayToStruct<T>(result);
        }

        return ByteArrayToStruct<T>(data);
    }

    public static (T[], int) ToArray<T>(byte[] data) where T : struct
    {
        T[] values = UnmarshalByteArrayToStructArray<T>(data, HEADER_SIZE);
        int id = GetPlayerNumber(data);

        return (values, id);
    }

    public static string ToString(byte[] data)
    {
       return Encoding.UTF8.GetString(data, 0, data.Length);
    }

    public static string GetMessageData(byte[] data)
    {
        return Encoding.UTF8.GetString(data, HEADER_SIZE, data.Length - HEADER_SIZE);
    }

    public static T GetMessageType<T>(byte[] data) where T : struct, Enum
    {
        string header = Encoding.UTF8.GetString(data, 0, MESSAGE_SIZE);
        return Enum.Parse<T>(header.TrimEnd());
    }

    public static int GetPlayerNumber(byte[] data)
    {
        string id = Encoding.UTF8.GetString(data, MESSAGE_SIZE, PLAYER_ID_LENGTH);
        return int.Parse(id.TrimEnd());
    }

    public static void ChangeMessage<T>(T messageName, ref byte[] data) where T : struct, Enum
    {
        var header = CreateHeader(messageName);
        for (int i = 0; i < MESSAGE_SIZE; i++)
        {
            data[i] = header[i];
        }
    }

    public static void ChangePlayerNumber(int playerNumber, ref byte[] data)
    {
        var idStr = CreatePlayerIDString(playerNumber);
        var id = Encoding.UTF8.GetBytes(idStr);

        for (int i = MESSAGE_SIZE; i < HEADER_SIZE; i++)
        {
            data[i] = id[i];
        }
    }

    public static byte[] GetByteDataFromMessage(byte[] data)
    {       
        byte[] result = new byte[data.Length - HEADER_SIZE];
        Array.Copy(data, HEADER_SIZE, result, 0, result.Length);
        return result;
    }

    public static byte[] StructToByteArray<T>(ref T str) where T : struct
    {
        int size = Marshal.SizeOf(str);
        byte[] arr = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }

        return arr;
    }

    static T ByteArrayToStruct<T>(byte[] arr) where T : struct
    {
        T str;
        int size = Marshal.SizeOf(typeof(T));
        IntPtr ptr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.Copy(arr, 0, ptr, size);
            str = Marshal.PtrToStructure<T>(ptr);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }

        return str;
    }

    static byte[] MarshalStructArrayToByteArray<T>(T[] structArray) where T : struct
    {
        int structSize = Marshal.SizeOf(typeof(T));
        int arraySize = structSize * structArray.Length;
        byte[] byteArray = new byte[arraySize];
        IntPtr ptr = Marshal.AllocHGlobal(arraySize);

        try
        {
            for (int i = 0; i < structArray.Length; i++)
            {
                IntPtr structPtr = new IntPtr(ptr.ToInt64() + i * structSize);
                Marshal.StructureToPtr(structArray[i], structPtr, false);
            }
            Marshal.Copy(ptr, byteArray, 0, arraySize);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }

        return byteArray;
    }

    static T[] UnmarshalByteArrayToStructArray<T>(byte[] byteArray, int startIndex) where T : struct
    {
        int structSize = Marshal.SizeOf(typeof(T));
        int arrayLength = (byteArray.Length - startIndex) / structSize;
        T[] structArray = new T[arrayLength];

        IntPtr ptr = Marshal.AllocHGlobal(byteArray.Length - startIndex);

        try
        {
            Marshal.Copy(byteArray, startIndex, ptr, byteArray.Length - startIndex);

            for (int i = 0; i < arrayLength; i++)
            {
                IntPtr structPtr = new IntPtr(ptr.ToInt64() + i * structSize);
                structArray[i] = Marshal.PtrToStructure<T>(structPtr);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }

        return structArray;
    }

    public static byte[] CombineByteArrays(byte[] array1, byte[] array2)
    {
        byte[] combinedArray = new byte[array1.Length + array2.Length];

        Buffer.BlockCopy(array1, 0, combinedArray, 0, array1.Length);
        Buffer.BlockCopy(array2, 0, combinedArray, array1.Length, array2.Length);

        return combinedArray;
    }
}