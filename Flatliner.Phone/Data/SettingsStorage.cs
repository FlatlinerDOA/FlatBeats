namespace Flatliner.Phone.Data
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO.IsolatedStorage;

    public class SettingsStorage : IStorage
    {
        public T Load<T>(string key)
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains(key))
            {
                return (T)Convert.ChangeType(IsolatedStorageSettings.ApplicationSettings[key], typeof(T), CultureInfo.InvariantCulture);
            }

            return default(T);
        }

        public void Save<T>(string key, T value)
        {
            this.Delete(key);
            if (!EqualityComparer<T>.Default.Equals(value, default(T)))
            {
                IsolatedStorageSettings.ApplicationSettings.Add(key, value);
            }
        }

        public void Delete(string key)
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains(key))
            {
                IsolatedStorageSettings.ApplicationSettings.Remove(key);
            }
        }

        public void DeleteAll()
        {
            IsolatedStorageSettings.ApplicationSettings.Clear();
        }
    }
}
