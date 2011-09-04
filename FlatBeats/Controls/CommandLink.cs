namespace FlatBeats.Controls
{
    using System.Windows.Input;

    using FlatBeats.ViewModels;

    public class CommandLink : ViewModel, ICommandLink
    {
        public ICommand Command
        {
            get;
            set;
        }

        private string text;

        public string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                if (this.text == value)
                {
                    return;
                }

                this.text = value;
                this.OnPropertyChanged("Text");
            }
        }

        public string IconUri
        {
            get;
            set;
        }

        public bool HideWhenInactive
        {
            get;
            set;
        }
    }
}