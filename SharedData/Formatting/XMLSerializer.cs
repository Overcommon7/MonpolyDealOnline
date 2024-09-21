using System.Diagnostics;
using System.Xml.Serialization;

public class XMLSerializer
{
    public static void Save<T>(string path, ref T obj)
    {
        if (File.Exists(path)) File.Delete(path);
        XmlSerializer serializer = new XmlSerializer(typeof(T));
        using (var fs = File.Create(path))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                serializer.Serialize(sw, obj);
            }
        }
    }

    public static bool Load<T>(string path, ref T obj)
    {
        if (!File.Exists(path)) return false;
        XmlSerializer serializer = new XmlSerializer(typeof(T));
        using (Stream stream = new FileStream(path, FileMode.Open))
        {
            try
            {
                obj = (T)serializer.Deserialize(stream);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }
        return true;
    }

    public static byte[] SaveObjectToXMLMemory<T>(ref T obj)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(T));
        using (MemoryStream ms = new MemoryStream())
        {
            serializer.Serialize(ms, obj);
            return ms.ToArray();
        }
    }

    public static bool LoadObjectFromXMLMemory<T>(byte[] data, ref T obj)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(T));
        using (MemoryStream ms = new MemoryStream(data))
        {
            try
            {
                obj = (T)serializer.Deserialize(ms);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }
        return true;
    }
}

