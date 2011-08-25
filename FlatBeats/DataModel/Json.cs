
namespace EightTracks.DataModel
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Json;
    using System.Text;

    public class Json
    {
        public static string Serialize<T>(T obj)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            string retVal; 
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, obj);
                retVal = Encoding.UTF8.GetString(ms.ToArray(), 0, (int)ms.Length);
            }

            return retVal;
        }

        public static T Deserialize<T>(string json)
        {
            T obj = default(T);
            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                obj = (T)serializer.ReadObject(ms);
                ms.Close();
            }

            return obj;
        }
    }
}
