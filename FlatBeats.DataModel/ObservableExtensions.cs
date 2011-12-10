using System;

namespace FlatBeats.DataModel
{
    using System.Threading;

    using Microsoft.Phone.Reactive;

    public static class ObservableExtensions
    {
        public static IObservable<T> DisposeOnCompletion<T>(this IObservable<T> items) where T : IDisposable
        {
            var list = new CompositeDisposable();
            return items.Do(item => list.Add(item)).Finally(list.Dispose);
        }

        public static IObservable<T> Wait<T>(this IObservable<T> items, ManualResetEvent resetEvent, TimeSpan timeout)
        {
            return from wait in Observable.Start(() => resetEvent.WaitOne(timeout)).Finally(() => resetEvent.Set()) 
                   from item in items 
                   select item;
        }
    }
}
