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
    public class PanelViewModel : ViewModel
    {
        private string message;

        public string Message
        {
            get
            {
                return this.message;
            }
            set
            {
                if (this.message == value)
                {
                    return;
                }

                this.message = value;
                this.OnPropertyChanged("Message");
            }
        }

        private bool showMessage;

        public bool ShowMessage
        {
            get
            {
                return this.showMessage;
            }
            set
            {
                if (this.showMessage == value)
                {
                    return;
                }

                this.showMessage = value;
                this.OnPropertyChanged("ShowMessage");
            }
        }

        public virtual void ShowError(Exception error)
        {
            this.Message = "Sorry, something went wrong, you might want to try again later.";
            this.ShowMessage = true;
        }
    }
}
