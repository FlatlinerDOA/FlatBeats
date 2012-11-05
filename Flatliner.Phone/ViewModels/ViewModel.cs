namespace Flatliner.Phone.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;

    public abstract class ViewModel : INotifyPropertyChanged
    {
        private static bool? isInDesignMode;


        /// <summary>
        /// Gets or sets a value indicating whether the application is loaded in a designer (DesignerProperties.IsInDesignTool).
        /// </summary>
        public static bool IsInDesignMode
        {
            get
            {
                if (!isInDesignMode.HasValue)
                {
                    isInDesignMode = DesignerProperties.IsInDesignTool;
                }

                return isInDesignMode.GetValueOrDefault();
            }
        }

        ////private static readonly Dictionary<Func<string>, string> Names = new Dictionary<Func<string>, string>();

        ////protected void OnPropertyChanged(Func<string> key)
        ////{
        ////    if (PropertyChanged != null)
        ////    {
        ////        string propertyName;
        ////        if (!Names.TryGetValue(key, out propertyName))
        ////        {
        ////            propertyName = key();
        ////            lock (Names)
        ////            {
        ////                Names[key] = propertyName;
        ////            }
        ////        }
        ////        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        ////    }
        ////}

        ////protected static string Reg<T>(Expression<Func<T>> property)
        ////{
        ////    return ((MemberExpression)property.Body).Member.Name;
        ////}


        protected void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            this.OnPropertyChanged(((MemberExpression)propertyExpression.Body).Member.Name);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            var temp = this.PropertyChanged;
            if (temp == null)
            {
                return;
            }

            temp(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
