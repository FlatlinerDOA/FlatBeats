namespace Flatliner.Phone
{
    using System;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Windows;

    using Microsoft.Phone.Tasks;

    public static class LittleWatsonLog
    {
        #region Constants and Fields

        private const string LogFileName = "LittleWatson.txt";

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
            }
        }

        public static void DeleteLog()
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                SafeDeleteFile(store);
            }
        }

        private static void SafeDeleteFile(IsolatedStorageFile store)
        {
            try
            {
                store.DeleteFile(LogFileName);
            }
            catch (Exception)
            {
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

