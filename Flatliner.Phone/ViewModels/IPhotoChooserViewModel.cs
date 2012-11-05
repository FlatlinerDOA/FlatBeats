namespace Flatliner.Phone.ViewModels
{
    using System;
    using Microsoft.Phone.Reactive;
    using Microsoft.Phone.Tasks;

    public interface IPhotoChooserViewModel : IChooserViewModel<PhotoResult>
    {
        IObservable<PhotoRequest> PhotoRequests
        {
            get; 
        }
    }
}
