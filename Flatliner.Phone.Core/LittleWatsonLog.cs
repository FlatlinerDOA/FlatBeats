namespace Flatliner.Phone
{
    using System;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Security.Cryptography;
    using System.Text;
    using System.Windows;

    using Microsoft.Phone.Tasks;

    public static class LittleWatsonLog
    {
        #region Constants and Fields

        private const string LogFileName = "LittleWatson.txt";

        private const string IgnoreLogFileName = "LittleWatsonIgnore.txt";

        #endregion

        #region Methods

        public static void ReportException(Exception ex, string extra)
        {
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    SafeDeleteFile(store);

                    using (TextWriter output = new StreamWriter(store.CreateFile(LogFileName)))
                    {
                        output.WriteLine(extra);
                        output.WriteLine(ex.Message);
                        output.WriteLine();
                        output.WriteLine(ex.GetType().FullName);
                        output.WriteLine(ex.StackTrace);
                    }
                }
            }
            catch (Exception)
            {
                // Gotta catch em all!
            }
        }

        public static void DeleteLog()
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                SafeDeleteFile(store);
            }
        }


        public static bool IsExceptionIgnored(string data)
        {
            try
            {
                var hash = HashString(data);
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (store.FileExists(IgnoreLogFileName))
                    {
                        using (TextReader reader = new StreamReader(store.OpenFile(IgnoreLogFileName, FileMode.Open, FileAccess.Read, FileShare.None)))
                        {
                            var line = "_";
                            while (!string.IsNullOrEmpty(line))
                            {
                                line = reader.ReadLine();
                                if (line == hash)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Gotta catch em all!
            }

            return false;
        }

        public static void IgnoreException(string data)
        {
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (var outputStream = store.OpenFile(IgnoreLogFileName, FileMode.OpenOrCreate))
                    {
                        outputStream.Seek(0, SeekOrigin.End);
                        using (var output = new StreamWriter(outputStream))
                        {
                            var hash = HashString(data);
                            output.WriteLine(hash);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Gotta catch em all!
            }
        }

        private static string HashString(string data)
        {
             var hash = new SHA1Managed();
             var result = hash.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(result);
        }

        private static void SafeDeleteFile(IsolatedStorageFile store)
        {
            try
            {
                store.DeleteFile(LogFileName);
            }
            catch (Exception)
            {
                // Gotta catch em all!
            }
        }

        #endregion

        public static string ReadLog()
        {
            string contents = null;

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(LogFileName))
                {
                    using (TextReader reader = new StreamReader(store.OpenFile(LogFileName, FileMode.Open, FileAccess.Read, FileShare.None)))
                    {
                        contents = reader.ReadToEnd();
                    }

                    SafeDeleteFile(store);
                }
            }

            return contents;
        }
    }
}

