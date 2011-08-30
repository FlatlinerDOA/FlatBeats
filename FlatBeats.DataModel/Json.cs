
namespace FlatBeats.DataModel
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Serialization.Json;
    using System.Text;

    public class Json
    {
        public static string Serialize<T>(T obj)
        {
            string retVal; 
            using (var ms = new MemoryStream())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
                serializer.WriteObject(ms, obj);
                retVal = Encoding.UTF8.GetString(ms.ToArray(), 0, (int)ms.Length);
                ms.Close();
            }

            return retVal;
        }

        public static T Deserialize<T>(string json)
        {
            T obj = default(T);
            if (string.IsNullOrWhiteSpace(json))
            {
                return obj;
            }

            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                obj = (T)serializer.ReadObject(ms);
                ms.Close();
            }

            return obj;
        }

        public static T DeserializeAndClose<T>(Stream json)
        {
            T obj = default(T);
            using (json)
            {
#if DEBUG
                var data = new MemoryStream();
                json.CopyTo(data);
                var jsonText = Encoding.UTF8.GetString(data.ToArray(), 0, (int)data.Length);
                Debug.WriteLine(jsonText);
                json = data;
#endif

                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                obj = (T)serializer.ReadObject(json);
                json.Close();
            }

            return obj;
        }
    }
}
