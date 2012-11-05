namespace Flatliner.Phone.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Xml.Linq;

    /// <summary>
    /// Converts Data Contract instances to and from Xml.
    /// </summary>
    public static class Serializer
    {
        /// <summary>
        /// Deserializes the Xml data to the specified type using a <see cref="DataContractSerializer"/>.
        /// </summary>
        /// <param name="stream">
        /// The raw Xml data to be deserialized.
        /// </param>
        /// <typeparam name="T">
        /// The type of the data contract class to deserialize as.
        /// </typeparam>
        /// <returns>
        /// Returns a new instance of type <typeparamref name="T"/>.
        /// </returns>
        public static T DeserializeAs<T>(this Stream stream)
        {
            try
            {
                var serializer = new DataContractSerializer(typeof(T));
                return (T)serializer.ReadObject(stream);
            }
            catch (IOException)
            {
            }
            catch (SerializationException)
            {
            }

            return default(T);
        }

        /// <summary>
        /// Deserializes the Xml element to the specified type using a <see cref="DataContractSerializer"/>.
        /// </summary>
        /// <param name="element">
        /// The Xml data to be deserialized.
        /// </param>
        /// <typeparam name="T">
        /// The type of the data contract class to deserialize as.
        /// </typeparam>
        /// <returns>
        /// Returns a new instance of type <typeparamref name="T"/>.
        /// </returns>
        public static T DeserializeAs<T>(this XElement element)
        {
            try
            {
                var sz = new DataContractSerializer(typeof(T));
                using (var reader = element.CreateReader())
                {
                    return (T)sz.ReadObject(reader);
                }
            }
            catch (IOException)
            {
            }
            catch (SerializationException)
            {
            }

            return default(T);
        }

        /// <summary>
        /// Attempts to deserialize each element to the specified type and 
        /// returns only the instances that are deserialized successfully.
        /// </summary>
        /// <typeparam name="T">The type of data contract to attempt to deserailize to</typeparam>
        /// <param name="elements">The elements that are the root of the data contract Xml.</param>
        /// <returns>Returns an enumerable sequence of non-null instances of type <typeparamref name="T"/>.</returns>
        public static IEnumerable<T> OfDeserializedType<T>(this IEnumerable<XElement> elements)
        {
            var items = from element in elements
                        let result = element.DeserializeAs<T>()
                        where !EqualityComparer<T>.Default.Equals(result, default(T))
                        select result;

            return items.ToList().AsReadOnly();
        }

        /// <summary>
        /// Serializes a data contract to an Xml element using a <see cref="DataContractSerializer"/>
        /// </summary>
        /// <param name="value">
        /// The object to be serialized.
        /// </param>
        /// <typeparam name="T">
        /// The type of the object to be serialized.
        /// </typeparam>
        /// <returns>
        /// Returns a new XElement with the data from the input value.
        /// </returns>
        public static XElement SerializeToXElement<T>(this T value)
        {
            using (var stream = new MemoryStream())
            {
                value.SerializeTo(stream);
                return XElement.Load(stream);
            }
        }

        /// <summary>
        /// Serializes a data contract to the specified stream.
        /// </summary>
        /// <typeparam name="T">The data contract type to serialize to.</typeparam>
        /// <param name="value">The data contract instance to be serialized.</param>
        /// <param name="stream">The stream to write the serialized Xml to.</param>
        public static void SerializeTo<T>(this T value, Stream stream)
        {
            var sz = new DataContractSerializer(typeof(T));
            sz.WriteObject(stream, value);
            stream.Flush();
            stream.Seek(0, SeekOrigin.Begin);
        }

        /// <summary>
        /// Seralizes a data contract to a string.
        /// </summary>
        /// <typeparam name="T">The data contract type to serialize to.</typeparam>
        /// <param name="value">The data contract instance to be serialized.</param>
        /// <returns>Returns an Xml string of the object.</returns>
        public static string SerializeToString<T>(this T value)
        {
            using (var stream = new MemoryStream())
            {
                value.SerializeTo(stream);
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
