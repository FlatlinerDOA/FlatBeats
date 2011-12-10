using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace FlatBeats.DataModel.Data
{
    using System.Linq;

    using Microsoft.Phone.Reactive;

    public class RecentlyPlayedRepository : IAsyncRepository<MixContract>
    {
        private const string Key = "recentmixes.json";

        private readonly JsonFileRepository<MixesResponseContract> repository = new JsonFileRepository<MixesResponseContract>(_ => Key, "Recent");
        
        /// <summary>
        /// Initializes a new instance of the RecentlyPlayedRepository class.
        /// </summary>
        public RecentlyPlayedRepository() 
        {
        }

        public IObservable<Unit> SaveAsync(MixContract mix)
        {
            return from updatedList in this.repository.GetAsync(Key).Select(
                mixList =>
                    {
                        var duplicates = mixList.Mixes.Where(d => d.Id == mix.Id).ToList();
                        foreach (var duplicate in duplicates)
                        {
                            mixList.Mixes.Remove(duplicate);
                        }

                        mixList.Mixes.Insert(0, mix);

                        if (mixList.Mixes.Count > 10)
                        {
                            mixList.Mixes.Remove(mixList.Mixes.Last());
                        }

                        return mixList;
                    })
                   from unit in this.repository.SaveAsync(updatedList)
                   select unit;
        }

        public IObservable<Unit> DeleteAsync(MixContract item)
        {
            return from list in this.repository.GetAsync(Key).Select(
                mixList =>
                    {
                        var existing = mixList.Mixes.FirstOrDefault(m => m.Id == item.Id);
                        if (existing != null)
                        {
                            mixList.Mixes.Remove(existing);
                        }

                        return mixList;
                    })
                   from unit in this.repository.DeleteAsync(list)
                   select unit;
        }

        public IObservable<MixContract> GetAsync(string key)
        {
            return this.repository.GetAsync(Key).SelectMany(k => k.Mixes).Where(m => m.Id == key);
        }

        public IObservable<MixContract> GetAllAsync()
        {
            return this.repository.GetAsync(Key).SelectMany(response => response.Mixes);
        }
    }
}
