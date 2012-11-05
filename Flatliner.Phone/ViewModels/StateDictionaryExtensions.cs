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

namespace Flatliner.Phone.ViewModels
{
    using System.Collections.Generic;
    using System.Globalization;

    public static class StateDictionaryExtensions
    {
        public static T GetValueOrDefault<T>(this IDictionary<string, object> pageState, string key)
        {
            object value;
            if (pageState.TryGetValue(key, out value))
            {
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
                }
                catch (InvalidCastException) { }
            } 

            return default(T);
        }
    }
}
