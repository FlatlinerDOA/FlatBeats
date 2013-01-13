namespace Flatliner.Phone
{
    using System;
    using System.Linq;
    using System.Diagnostics;
    using System.Threading;

    using Microsoft.Phone.Reactive;
    using System.Collections.Generic;
    using Flatliner.Functional;

    public static class ObservableEx
    {
        public static readonly PortableUnit Unit = new PortableUnit();
 
        public static IEnumerable<string> NotNullOrEmpty(this IEnumerable<string> source)
        {
            return source.Where(x => !string.IsNullOrEmpty(x));
        }

        public static IObservable<string> NotNullOrEmpty(this IObservable<string> source)
        {
            return source.Where(x => !string.IsNullOrEmpty(x));
        }

        public static IEnumerable<T> NotNull<T>(this IEnumerable<T> source) where T : class
        {
            return source.Where(x => x != null);
        }

        public static IObservable<T> NotNull<T>(this IObservable<T> source) where T : class
        {
            return source.Where(x => x != null);
        }

        public static IObservable<T> Coalesce<T>(this IObservable<T> source, Func<T> defaultValue) where T : class
        {
            return source.Select(x => x ?? defaultValue());
        }

        public static IObservable<PortableUnit> ToUnit<T>(this IObservable<T> source)
        {
            return source.Select(_ => Unit);
        }

        public static IObservable<PortableUnit> SingleUnit()
        {
            return Observable.Return(Unit);
        }

        public static IObservable<T> DefaultIfEmpty<T>(this IObservable<T> source)
        {
            return source.DefaultIfEmpty(default(T));
        }

        public static IObservable<T> DefaultIfEmpty<T>(this IObservable<T> source, T defaultValue)
        {
            bool isEmpty = true;
            return source.Do(_ => isEmpty = false).Concat(Defer(() => isEmpty ? Observable.Return(defaultValue) : Observable.Empty<T>()));
        }

        public static IObservable<PortableUnit> DeferredStart(Action action)
        {
            return DeferredStart(action, Scheduler.Immediate);
        }

        public static IObservable<PortableUnit> DeferredStart(Action action, IScheduler scheduler)
        {
            return Observable.CreateWithDisposable<PortableUnit>(
                observer => scheduler.Schedule(
                    () =>
                        {
                            try
                            {
                                action();
                                observer.OnNext(new PortableUnit());
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

        public static IObservable<T> Defer<T>(Func<IObservable<T>> func)
        {
            return Observable.CreateWithDisposable<T>(observer => func().Subscribe(observer));
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

        public static IObservable<T> TakeUntil<T>(this IObservable<T> items, Predicate<T> until)
        {
            return items.TakeWhile(t => !until(t));
        }

        public static IObservable<T> TakeFirst<T>(this IObservable<T> items, Predicate<T> match)
        {
            return items.Where(t => match(t)).Take(1);
        }

        public static IObservable<T> Log<T>(this IObservable<T> items, string message, params object[] messageParameters )
        {
#if DEBUG
            return items.Do(
                t =>
                    { Debug.WriteLine(message, messageParameters); });
#else
            return items;
#endif
        }
    }
}
