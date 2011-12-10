using System;

namespace FlatBeats.DataModel.Data
{
    public interface IPaginatedAsyncRepository<T> where T : class
    {
        IObservable<T> GetPageAsync(int pageIndex, int pageSize);
    }
}
