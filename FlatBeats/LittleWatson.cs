namespace FlatBeats
{
    using System;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Windows;

    using Microsoft.Phone.Tasks;

    public class LittleWatson
    {
        #region Constants and Fields

        private const string LogFileName = "LittleWatson.txt";

        #endregion

        #region Methods

        internal static void CheckForPreviousException()
        {
            try
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

                if (contents != null)
                {
                    if (MessageBox.Show(
                            "A problem occurred the last time you ran this application. Would you like to send an email to report it?",
                            "Problem Report",
                            MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        EmailComposeTask email = new EmailComposeTask();

                        email.To = "flatlinerdoa@gmail.com";

                        email.Subject = "FlatBeats error report - 1.0";

                        email.Body = contents;

                        SafeDeleteFile(IsolatedStorageFile.GetUserStoreForApplication()); // line added 1/15/2011
                        email.Show();
                    }
                }
            }
            catch (Exception)
            {
            }

            finally
            {
                SafeDeleteFile(IsolatedStorageFile.GetUserStoreForApplication());
            }
        }

        internal static void ReportException(Exception ex, string extra)
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

                        output.WriteLine(ex.StackTrace);
                    }
                }
            }

            catch (Exception)
            {
            }
        }

        private static void SafeDeleteFile(IsolatedStorageFile store)
        {
            try
            {
                store.DeleteFile(LogFileName);
            }

            catch (Exception ex)
            {
            }
        }

        #endregion
    }
}

