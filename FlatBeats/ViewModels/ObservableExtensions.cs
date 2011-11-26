//--------------------------------------------------------------------------------------------------
// <copyright file="ObservableExtensions.cs" company="DNS Technology Pty Ltd.">
//   Copyright (c) 2011 DNS Technology Pty Ltd. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Windows.Threading;

    using Microsoft.Phone.Reactive;

    public static class ObservableExtensions
    {
        public static IObservable<T> FlowInTime<T>(this IObservable<T> source)
        {
            return from item in source
                   from interval in Observable.Interval(TimeSpan.FromMilliseconds(150)).Take(1)
                   select item;
        }

        public static void SetLastItem<T>(this IEnumerable<T> items) where T : ListItemViewModel
        {
            var last = items.LastOrDefault();
            if (last != null)
            {
                last.IsLastItem = true;
            }
        }

        public static IObservable<T> FlowIn<T>(this IObservable<T> source, int delay = 75)
        {
            bool hasBeenRun = false;
            return source.Do(
                _ =>
                    {
                        if (!hasBeenRun)
                        {
                            hasBeenRun = true;
                        }
                        else
                        {
                            Thread.Sleep(delay);
                        }
                    });
        }

        public static IObservable<T> FirstDo<T>(this IObservable<T> sequence, Action<T> firstAction)
        {
            bool hasBeenRun = false;
            return Observable.CreateWithDisposable<T>(
                observer => sequence.Subscribe(
                    _ =>
                    {
                        if (!hasBeenRun)
                        {
                            hasBeenRun = true;
                            firstAction(_);
                        }

                        observer.OnNext(_);
                    }, 
                    observer.OnError, 
                    observer.OnCompleted));
        }

        public static IObservable<TResult> FinallySelect<T, TResult>(this IObservable<T> sequence, Func<TResult> finalValue)
        {
            return Observable.CreateWithDisposable<TResult>(
                observer => sequence.Subscribe(
                    _ =>
                    {
                    },
                    observer.OnError,
                    () =>
                    { 
                        observer.OnNext(finalValue());
                        observer.OnCompleted();
                    }));
        }
    }
}
