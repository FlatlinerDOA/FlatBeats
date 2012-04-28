
namespace FlatBeats.Framework
{
    using System;
    using System.Linq;

    using FlatBeats.DataModel.Profile;
    using FlatBeats.DataModel.Services;

    using Microsoft.Phone.Reactive;

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
                            // TODO: HACK: Make load asynchronous
                            userSettings = ProfileService.GetSettingsAsync().FirstOrDefault();
                        }
                    }
                }

                return userSettings;
            }
        }
    }
}
