namespace Flatliner.Phone
{
    using System;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Windows;

    using Microsoft.Phone.Tasks;

    public static class LittleWatson
    {
        private static string reportTo;

        private static string title;

        private static int major;

        private static int minor;

        #region Methods

        public static void Initialize(Application app, string reportToEmailAddress, string applicationTitle, int majorVersion, int minorVersion)
        {
            app.UnhandledException -= ApplicationUnhandledException;
            app.UnhandledException += ApplicationUnhandledException;
            reportTo = reportToEmailAddress;
            title = applicationTitle;
            major = majorVersion;
            minor = minorVersion;
        }

        // Code to execute on Unhandled Exceptions
        private static void ApplicationUnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            ReportException(e.ExceptionObject, string.Empty);
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        public static void CheckForPreviousException()
        {
            try
            {
                var contents = LittleWatsonLog.ReadLog();
                if (contents != null && !LittleWatsonLog.IsExceptionIgnored(contents))
                {
                    if (MessageBox.Show(
                            "A problem occurred the last time you ran this application. Would you like to send an email to report it?",
                            "Problem Report",
                            MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        var email = new EmailComposeTask 
                        {
                            To = reportTo,
                            Subject = string.Format("{0} {1}.{2} Error Report", title, major, minor),
                            Body = contents
                        };

                        LittleWatsonLog.DeleteLog(); // line added 1/15/2011
                        email.Show();
                    }

                    LittleWatsonLog.IgnoreException(contents);
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                LittleWatsonLog.DeleteLog();
            }
        }

        public static void ReportException(Exception ex, string extra)
        {
            LittleWatsonLog.ReportException(ex, extra);
        }

        #endregion
    }
}

