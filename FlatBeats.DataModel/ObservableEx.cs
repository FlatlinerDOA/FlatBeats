using System;

namespace FlatBeats.DataModel
{
    using System.Threading;

    using Microsoft.Phone.Reactive;

    public static class ObservableEx
    {
        public static IObservable<Unit> DeferredStart(Action action)
        {
            return DeferredStart(action, Scheduler.Immediate);
        }

        public static IObservable<Unit> DeferredStart(Action action, IScheduler scheduler)
        {
            return Observable.CreateWithDisposable<Unit>(
                observer => scheduler.Schedule(
                    () =>
                        {
                            try
                            {
                                action();
                                observer.OnNext(new Unit());
                                observer.OnCompleted();
                            }
                            catch (Exception ex)
                            {
                                observer.OnError(ex);
                            }
                        }));
        }

        public static IObservable<T> DeferredStart<T>(Func<T> func)
        {
            return DeferredStart(func, Scheduler.Immediate);
        }

        public static IObservable<T> DeferredStart<T>(Func<T> func, IScheduler scheduler)
        {
            return Observable.CreateWithDisposable<T>(
                observer => scheduler.Schedule(
                    () =>
                    {
                        try
                        {
                            observer.OnNext(func());
                            observer.OnCompleted();
                        }
                        catch (Exception ex)
                        {
                            observer.OnError(ex);
                        }
                    }));
        }

        public static IObservable<T> DisposeOnCompletion<T>(this IObservable<T> items) where T : IDisposable
        {
            var list = new CompositeDisposable();
            return items.Do(item => list.Add(item)).Finally(list.Dispose);
        }

        public static IObservable<T> Wait<T>(this IObservable<T> items, ManualResetEvent resetEvent, TimeSpan timeout)
        {
            return from wait in DeferredStart(() => resetEvent.WaitOne(timeout), Scheduler.Immediate).Finally(() => resetEvent.Set())
                   where wait
                   from item in items 
                   select item;
        }
    }
}
