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
            obj = (T)serializer.Deserialize(stream);
        }
        return true;
    }
}

