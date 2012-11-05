
namespace Flatliner.Phone.Data
{
    using System;
    public interface IStorage
    {
        T Load<T>(string key);

        void Save<T>(string key, T value);

        void Delete(string key);

        void DeleteAll();
    }
}
