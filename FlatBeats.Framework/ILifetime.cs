
namespace FlatBeats.Framework
{
    using System;
    using Microsoft.Phone.Reactive;

    public interface ILifetime
    {
        bool IsLoaded { get; }

        IObservable<Unit> LoadAsync();

        void Unload();
    }

    public interface ILifetime<T>
    {
        bool IsLoaded { get; }
        
        IObservable<Unit> LoadAsync(T loadState);

        void Unload();
    }
}
