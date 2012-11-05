namespace Flatliner.Phone.Core
{
    using System;
    using Flatliner.Functional;

    using Microsoft.Phone.Reactive;

    public static class ObserveExtensions
    {
        public static IObservable<T> ToObservable<T>(this Observe<T> source)
        {
            return Observable.Create<T>(o => () => source.Subscribe(o.OnNext, o.OnError, o.OnCompleted));
        }

        public static Observe<T> ToFunctional<T>(this IObservable<T> source)
        {
            return o => source.Subscribe(value => o(new Some<T>(value)), ex => o(new Error<T>(ex)), () => o(new None<T>()));
        }

        public static Observe<T> ObserveOnDispatcher<T>(this Observe<T> source)
        {
            return source.ObserveOn(Scheduler.Dispatcher.Schedule);
        }
    }
}
