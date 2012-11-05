namespace FlatBeats.DataModel.Data
{
    using System;
    using Flatliner.Functional;

    public interface IAsyncRepository<T> where T : class
    {
        IObservable<PortableUnit> SaveAsync(T item);

        IObservable<PortableUnit> DeleteAsync(T item);

        IObservable<T> GetAsync(string key);

        IObservable<T> GetAllAsync();
    }
}
