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

namespace FlatBeats.ViewModels
{
    using Flatliner.Phone.ViewModels;

    public class ListItemViewModel : ViewModel
    {
        private bool isFirstItem;

        public bool IsFirstItem
        {
            get
            {
                return this.isFirstItem;
            }
            set
            {
                if (this.isFirstItem == value)
                {
                    return;
                }

                this.isFirstItem = value;
                this.OnPropertyChanged("IsFirstItem");
            }
        }

        private bool isLastItem;

        public bool IsLastItem
        {
            get
            {
                return this.isLastItem;
            }
            set
            {
                if (this.isLastItem == value)
                {
                    return;
                }

                this.isLastItem = value;
                this.OnPropertyChanged("IsLastItem");
            }
        }
    }
}
