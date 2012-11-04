namespace Flatliner.UnitTests
{
    using System;
    using System.Reactive.Linq;

    using Flatliner.Functional;

    public static class ObserveExtensions
    {
        public static IObservable<T> ToObservable<T>(this Observable<T> source)
        {
            return Observable.Create<T>(o => () => source.Subscribe(o.OnNext, o.OnError, o.OnCompleted));
        } 
    }
}