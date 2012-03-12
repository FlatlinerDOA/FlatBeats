namespace FlatBeats.DataModel.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using FlatBeats.DataModel;
    using Microsoft.Phone.Reactive;

    public interface IProfileService
    {
        string UserId { get; }

        bool IsLoggedIn { get; }

        IObservable<Unit> InitializeAsync();

        IObservable<Unit> AuthenticateAsync(UserCredentialsContract userCredentials);

        IObservable<IList<MixContract>> GetLikedMixesAsync(int pageNumber, int pageSize);
    }
}
