using System.Xml.Serialization;
using System.IO;
using System.Text;

public class XMLSerializer
{
    public static void Save<T>(string path, string fileName, object o)
    {
        string xmlPath = string.Format("{0}/{1}", path, fileName);
        Save<T>(xmlPath, o);
    }

    public static void Save<T>(string path, object o)
    {
        XmlSerializer x = new XmlSerializer(typeof(T));
        using (var fs = new FileStream(path, FileMode.Create))
        {
            var streamWrite = new StreamWriter(fs, Encoding.UTF8);
            x.Serialize(streamWrite, o);
        }
    }

    public static object Read<T>(string path)
    {
        XmlSerializer x = new XmlSerializer(typeof(T));
        using (var fs = new FileStream(path, FileMode.Open))
        {
            var streamReader = new StreamReader(fs, Encoding.UTF8);
            return (T)x.Deserialize(streamReader);
        }
    }

    public static object Read<T>(byte[] datas)
    {
        XmlSerializer x = new XmlSerializer(typeof(T));
        using (var ms = new MemoryStream(datas))
        {
            var streamReader = new StreamReader(ms, Encoding.UTF8);
            return (T)x.Deserialize(streamReader);
        }
    }
}