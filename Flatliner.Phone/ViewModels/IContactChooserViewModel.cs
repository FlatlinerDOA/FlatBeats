
namespace Flatliner.Phone.ViewModels
{
    using System;
    using Microsoft.Phone.Tasks;

    public interface IEmailAddressChooserViewModel : IChooserViewModel<EmailResult>
    {
        IObservable<EmailComposeTask> EmailAddressRequests
        {
            get;
        }
    }
}
