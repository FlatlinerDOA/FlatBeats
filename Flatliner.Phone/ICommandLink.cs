namespace Flatliner.Phone
{
    using System;
    using System.Windows.Input;

    public interface ICommandLink
    {
        ICommand Command
        {
            get;
        }

        string Text
        {
            get;
        }

        Uri IconUrl
        {
            get;
        }

        bool HideWhenInactive
        {
            get;
        }
    }
}