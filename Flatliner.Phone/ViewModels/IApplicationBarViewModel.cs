
namespace Flatliner.Phone.ViewModels
{
    using System.Collections.ObjectModel;
    using Flatliner.Phone.Behaviors;
    using Flatliner.Portable;

    public interface IApplicationBarViewModel
    {
        ObservableCollection<ICommandLink> ApplicationBarButtonCommands
        {
            get;
        }

        ObservableCollection<ICommandLink> ApplicationBarMenuCommands
        {
            get;
        }
    }
}
