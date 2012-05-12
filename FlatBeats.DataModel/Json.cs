
namespace FlatBeats.DataModel
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;

    using FlatBeats.DataModel.Services;

    public class Json<T> : ISerializer<T> where T : class 
    {
        private static readonly DataContractJsonSerializer Serializer;
     
        public static readonly Json<T> Instance;
 
        static Json()
        {
            Serializer = new DataContractJsonSerializer(typeof(T));
            Instance = new Json<T>();
        }

        public string SerializeToString(T obj)
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

        public T DeserializeFromString(string json) 
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
                catch (SerializationException)
                {
                }
                catch (ArgumentException)
                {
                }
            }

            return obj;
        }

        public T DeserializeFromStream(Stream json)
        {
            T obj = default(T);
            using (json)
            {
                try
                {
                    ////var data = new MemoryStream();
                    ////json.CopyTo(data);

////#if DEBUG
////                    var jsonText = Encoding.UTF8.GetString(data.ToArray(), 0, (int)data.Length);
////                    foreach (var line in
////                            jsonText.Replace("{", "\r\n{\r\n").Replace("}", "\r\n}\r\n").Split(
////                                new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
////                    {
////                        Debug.WriteLine(line);
////                    }

////#endif
                    ////obj = (T)Serializer.ReadObject(data);
                    obj = (T)Serializer.ReadObject(json);
                    json.Close();
                }
                catch (SerializationException)
                {
                }
                catch (ArgumentException)
                {
                }
            }

            return obj;
        }

        public void SerializeToStream(T item, Stream stream)
        {
            if (item == null)
            {
                return;
            }

            Serializer.WriteObject(stream, item);
        }
    }
}
