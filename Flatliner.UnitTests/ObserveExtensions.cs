namespace Flatliner.UnitTests
{
    using System;
    using System.Reactive.Linq;
    using System.Runtime.CompilerServices;

    using Flatliner.Functional;

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

        public static ObservableAwaiter<T> GetAwaiter<T>(this Observe<T> source)
        {
            return new ObservableAwaiter<T>(source);
        }
    }

    public sealed class ObservableAwaiter<T> : INotifyCompletion
    {
        private readonly Observe<T> observable;

        private bool isCompleted;

        private Action onCompleted;

        private IMaybe<T> result;

        public ObservableAwaiter(Observe<T> observable)
        {
            this.result = new None<T>();
            this.onCompleted = null;
            this.isCompleted = false;
            this.observable = observable;
        }

        public bool IsCompleted
        {
            get
            {
                return this.isCompleted;
            }
        }

        public void OnCompleted(Action continuation)
        {
            this.onCompleted = continuation;
            this.observable(this.OnNext);
        }

        private void OnNext(IMaybe<T> newResult)
        {
            if (this.isCompleted)
            {
                return;
            }

            if (newResult is None<T>)
            {
                this.isCompleted = true;
                if (this.onCompleted != null)
                {
                    this.onCompleted();
                }

                return;
            }

            this.result = newResult;
        }

        public T GetResult()
        {
            if (this.result is None<T>)
            {
                return default(T);
            }

            return this.result.Value;
        }
    }
}