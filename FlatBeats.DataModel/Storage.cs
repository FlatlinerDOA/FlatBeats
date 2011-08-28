namespace FlatBeats.DataModel
{
    using System;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Text;

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
                using (var stream = new IsolatedStorageFileStream(file, FileMode.Create, storage))
                {
                    using (var writer = new StreamWriter(stream, Encoding.UTF8))
                    {
                        writer.Write(text);
                        writer.Flush();
                        writer.Close();
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
                if (!storage.FileExists(file))
                {
                    return null;
                }

                using (var stream = new IsolatedStorageFileStream(file, FileMode.Open, storage))
                {
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
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
                using (var stream = new IsolatedStorageFileStream(file, FileMode.Create, storage))
                {
                    data.CopyTo(stream);
                    stream.Flush();
                    stream.Close();
                }
            }
        }

        public static void Delete(string file)
        {
            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                storage.DeleteFile(file);
            }
        }
    }
}
