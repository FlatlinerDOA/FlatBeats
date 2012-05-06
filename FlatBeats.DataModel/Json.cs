
namespace FlatBeats.DataModel
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;

    public static class Json<T> where T : class 
    {
        private static readonly DataContractJsonSerializer Serializer;
        
        static Json()
        {
            Serializer = new DataContractJsonSerializer(typeof(T));
        }

        public static string Serialize(T obj)
        {
            if (obj == null)
            {
                return null;
            }

            string retVal; 
            using (var ms = new MemoryStream())
            {
                Serializer.WriteObject(ms, obj);
                retVal = Encoding.UTF8.GetString(ms.ToArray(), 0, (int)ms.Length);
                ms.Close();
            }

            return retVal;
        }

        public static T Deserialize(string json) 
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
                    obj = (T)Serializer.ReadObject(ms);
                    ms.Close();
                }
                catch (SerializationException) { }
                catch (ArgumentException) { }
            }

            return obj;
        }

        public static T DeserializeAndClose(Stream json)
        {
            T obj = default(T);
            using (json)
            {
                try
                {
                    var data = new MemoryStream();
                    json.CopyTo(data);

////#if DEBUG
////                    var jsonText = Encoding.UTF8.GetString(data.ToArray(), 0, (int)data.Length);
////                    foreach (var line in
////                            jsonText.Replace("{", "\r\n{\r\n").Replace("}", "\r\n}\r\n").Split(
////                                new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
////                    {
////                        Debug.WriteLine(line);
////                    }

////#endif
                    obj = (T)Serializer.ReadObject(data);
                    json.Close();
                }
                catch (SerializationException) { }
                catch (ArgumentException)
                {
                }
            }

            return obj;
        }

        public static void SerializeToStream(T item, IsolatedStorageFileStream stream)
        {
            throw new NotImplementedException();
        }
    }
}
