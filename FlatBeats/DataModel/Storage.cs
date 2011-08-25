namespace FlatBeats.DataModel
{
    using System;
    using System.IO;
    using System.IO.IsolatedStorage;

    public static class Storage
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool Exists(string file)
        {
            if (file == null)
            {
                return false;
            }

            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return storage.FileExists(file);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="text"></param>
        public static void Save(string file, string text)
        {
            if (file == null)
            {
                return;
            }

            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var stream = storage.CreateFile(file))
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(text);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string Load(string file)
        {
            if (file == null)
            {
                return null;
            }

            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var stream = storage.CreateFile(file))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        public static void Save(string file, Stream data)
        {
            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var stream = storage.CreateFile(file))
                {
                    data.CopyTo(stream);
                    stream.Flush();
                    stream.Close();
                }
            }
        }
    }
}
