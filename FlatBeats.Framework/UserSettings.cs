using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace FlatBeats.Framework
{
    using FlatBeats.DataModel.Profile;
    using FlatBeats.DataModel.Services;

    public static class UserSettings
    {

        private static object syncRoot = new object();

        private static SettingsContract userSettings;

        public static SettingsContract Current
        {
            get
            {
                if (userSettings == null)
                {
                    lock (syncRoot)
                    {
                        if (userSettings == null)
                        {
                            userSettings = ProfileService.GetSettings();
                        }
                    }
                }

                return userSettings;
            }
        }
    }
}
