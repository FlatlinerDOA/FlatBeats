
namespace FlatBeats.DataModel
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Runtime.Serialization.Json;
    using System.Text;

    public class Json
    {
        public static string Serialize<T>(T obj, params Type[] knownTypes) where T : class 
        {
            if (obj == null)
            {
                return null;
            }

            string retVal; 
            using (var ms = new MemoryStream())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType(), knownTypes);
                serializer.WriteObject(ms, obj);
                retVal = Encoding.UTF8.GetString(ms.ToArray(), 0, (int)ms.Length);
                ms.Close();
            }

            return retVal;
        }

        public static T Deserialize<T>(string json, params Type[] knownTypes) where T : class 
        {
            T obj = null;
            if (string.IsNullOrWhiteSpace(json))
            {
                return obj;
            }

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                try
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T), knownTypes);
                    obj = (T)serializer.ReadObject(ms);
                    ms.Close();
                }
                catch (ArgumentException) { }
            }

            return obj;
        }

        public static T DeserializeAndClose<T>(Stream json) where T : class 
        {
            T obj = default(T);
            using (json)
            {
                try
                {
#if DEBUG
                    var data = new MemoryStream();
                    json.CopyTo(data);
                    var jsonText = Encoding.UTF8.GetString(data.ToArray(), 0, (int)data.Length);
                    foreach (
                        var line in
                            jsonText.Replace("{", "\r\n{\r\n").Replace("}", "\r\n}\r\n").Split(
                                new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        Debug.WriteLine(line);
                    }

                    json = data;
#endif

                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                    obj = (T)serializer.ReadObject(json);
                    json.Close();
                }
                catch (ArgumentException)
                {
                    
                }
            }

            return obj;
        }

        public static void SerializeToStream<T>(T item, IsolatedStorageFileStream stream)
        {
            throw new NotImplementedException();
        }
    }
}
