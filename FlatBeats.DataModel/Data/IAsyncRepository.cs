namespace FlatBeats.DataModel.Data
{
    using System;
    using Microsoft.Phone.Reactive;

    public interface IAsyncRepository<T> where T : class
    {
        IObservable<Unit> SaveAsync(T item);

        IObservable<Unit> DeleteAsync(T item);

        IObservable<T> GetAsync(string key);

        IObservable<T> GetAllAsync();
    }
}
