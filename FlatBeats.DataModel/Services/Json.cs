﻿namespace FlatBeats.DataModel.Services
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Xml;

    public sealed class Json<T> : ISerializer<T> where T : class 
    {
        private readonly DataContractJsonSerializer Serializer;

        public static readonly ISerializer<T> Instance = new Json<T>();
 
        public Json()
        {
            this.Serializer = new DataContractJsonSerializer(typeof(T));
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
                this.Serializer.WriteObject(ms, obj);
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
                        obj = (T)this.Serializer.ReadObject(ms);
                        ms.Close();
                    }
                    catch (InvalidDataContractException)
                    {
                    }
                    catch (InvalidCastException)
                    {
                    }
                    catch (SerializationException)
                    {
                    }
                    catch (ArgumentException)
                    {
                    }
                    catch (XmlException)
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
                        data.Position = 0;
                        obj = (T)this.Serializer.ReadObject(data);
#else
                        obj = (T)this.Serializer.ReadObject(json);
#endif
                    }
                    catch (InvalidDataContractException)
                    {
                    }
                    catch (InvalidCastException)
                    {
                    }
                    catch (SerializationException)
                    {
                    }
                    catch (ArgumentException)
                    {
                    }
                    catch (XmlException)
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

            this.Serializer.WriteObject(stream, item);
        }
    }
}
